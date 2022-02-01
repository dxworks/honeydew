const _package = require('../package.json')
const path = require('path')
const os = require('os')
const fs = require('fs')
const axios = require('axios')
const unzipper = require('unzipper')

const {execSync} = require('child_process')

const {Command} = require("commander")

exports.honeydewCommand = new Command()
  .name('honeydew')
  .description('C# Analysis Tool')
  .option('-wd --working-directory', 'Selects the directory where Honeydew will store the results folder.' +
    ` Defaults to the location where Honeydew is installed: ${path.resolve(os.homedir(), '.dxw', 'honeydew', _package.version)}. If set to true it will use the current working directory process.cwd()`,
    false)
  .allowUnknownOption()
  .action(runHoneydew)

async function runHoneydew(options) {
  const version = _package.version
  console.log('Checking if Honeydew ' + version + ' is installed')
  const currentVersionFolder = path.resolve(os.homedir(), '.dxw', 'honeydew', _package.version)
  const honeydewExePath = path.resolve(currentVersionFolder, 'Honeydew');

  if (!fs.existsSync(honeydewExePath)) {
    fs.mkdirSync(currentVersionFolder, {recursive: true})
    let platformName = getPlatformName();
    console.log(`Downloading Honeydew ${_package.version}`)
    const downloadedFile = await downloadFile(`https://github.com/dxworks/honeydew/releases/download/v${_package.version}/honeydew-${platformName}.zip`, path.resolve(currentVersionFolder, 'honeydew.zip'))
    console.log(`Download Finished`)
    console.log('Installing...')
    await unzip(downloadedFile, {path: currentVersionFolder, overwriteRootDir: true})
    fs.rmSync(downloadedFile, {force: true})
    console.log('Install Finished')
    fs.chmodSync(honeydewExePath, '755')
  } else {
    console.log('Found local installation')
  }

  const args = [...process.argv];
  let index = args.indexOf('honeydew'); //if it is called from dxw cli
  if (index === -1)
    index = 1
  args.splice(0, index + 1);

  console.log('Workdir is ' + options.workingDirectory)
  await execSync(`${honeydewExePath} ${args.join(' ')}`, {cwd: options?.workingDirectory? process.cwd(): currentVersionFolder, stdio: 'inherit'})
}

function getPlatformName() {
  switch (process.platform) {
    case 'win32':
      return 'win'
    case 'darwin':
      return 'osx'
    case 'linux':
      return 'linux'
    default:
      throw Error('Honeydew can only be installed on Windows, Mac or Linux systems')
  }
}

async function downloadFile(url, filename, payload, progressBar) {
  const file = fs.createWriteStream(filename, 'utf-8')
  let receivedBytes = 0

  const {data, headers, status} = await axios.get(url,
    {
      method: 'GET',
      responseType: 'stream',
    })

  const totalBytes = headers['content-length'] ? +headers['content-length'] : 0

  return new Promise((resolve, reject) => {
    if (status !== 200) {
      return reject('Response status was ' + status)
    }
    progressBar?.start(totalBytes, 0, payload)
    data
      .on('data', (chunk) => {
        receivedBytes += chunk.length
        progressBar?.update(receivedBytes, payload)
      })
      .pipe(file)
      .on('finish', () => {
        file.close()
        resolve(filename)
      })
      .on('error', (err) => {
        fs.unlinkSync(filename)
        progressBar?.stop()
        return reject(err)
      })
  })
}

async function unzip(zipFileName, options) {
  return new Promise((resolve, reject) => {
    if (options?.overwriteRootDir) {
      fs.createReadStream(zipFileName)
        .pipe(unzipper.Parse())
        .on('entry', function (entry) {
          const fullPathName = path.resolve(options.path, entry.path.substring(entry.path.indexOf('/') + 1, entry.path.length))
          if (entry.type === 'Directory') {
            if (!fs.existsSync(fullPathName))
              fs.mkdirSync(fullPathName, {recursive: true})
          } else
            entry.pipe(fs.createWriteStream(fullPathName))
        })
        .on('finish', () => {
          resolve()
        })
        .on('error', reject)
    } else {
      fs.createReadStream(zipFileName)
        .pipe(unzipper.Extract(options))
        .on('finish', () => {
          resolve()
        })
        .on('error', reject)
    }
  })
}
