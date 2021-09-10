@ECHO OFF
REM the script will take the desired files from the provided source folder and copy it to the destination folder 
REM if no destination folder is provided, then the destination folder will be '../e2e_results'

IF [%1] == [] (
	ECHO "Usage %0 <source_results_folder> [destination_folder]"
	EXIT 1
)

IF NOT [%3] == [] (
	ECHO "Usage %0 <source_results_folder> [destination_folder]"
	EXIT 1
)

IF [%2] == [] (
	SET dst_folder=..\e2e_results
)
IF NOT [%2] == [] (
	SET dst_folder=%~2
)

SET src_folder=%~1

IF NOT EXIST %dst_folder% (
	mkdir %dst_folder%
)

CALL :copy_file honeydew.csv
CALL :copy_file honeydew_intermediate.csv
CALL :copy_file honeydew.json
CALL :copy_file honeydew_intermediate.json
CALL :copy_file honeydew_cyclomatic.json
CALL :copy_file honeydew_cyclomatic_intermediate.json
CALL :copy_file honeydew_namespaces.json
CALL :copy_file honeydew_file_relations.csv
CALL :copy_file honeydew_file_relations_intermediate.csv
CALL :copy_file honeydew_file_relations_all.csv
CALL :copy_file honeydew_file_relations_all_intermediate.csv


EXIT /B 0
 
:copy_file

IF NOT EXIST "%src_folder%/%1" ( 
	ECHO File %1 does not exist in %src_folder%
	EXIT 2
)

echo Copying "%src_folder%/%1" to "%dst_folder%/%1"
 
COPY "%src_folder%\%1" %dst_folder%

EXIT /B 0
