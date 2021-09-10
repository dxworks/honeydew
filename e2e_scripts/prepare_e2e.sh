# the script will take the desired files from the provided source folder and copy it to the destination folder 
# if no destination folder is provided, then the destination folder will be '../e2e_results'
if test $# -eq 1
then
	dst_folder="../e2e_results"
elif test $# -eq 2
then
	dst_folder="$2"
else
  echo "Usage $0 <source_results_folder> [destination_folder]"; exit 1
fi

src_folder="$1"

mkdir -p $dst_folder

copy_file () {
	if ! test -f "$src_folder/$1"
	then 
		echo "File $1 does not exist in $src_folder"; exit 2
	fi

	echo "Copying $src_folder/$1 to $dst_folder/$1"
	
	cp -R "$src_folder/$1" "$dst_folder/$1"
}

copy_file "honeydew.csv"
copy_file "honeydew_intermediate.csv"
copy_file "honeydew.json"
copy_file "honeydew_intermediate.json"
copy_file "honeydew_cyclomatic.json"
copy_file "honeydew_cyclomatic_intermediate.json"
copy_file "honeydew_namespaces.json"
copy_file "honeydew_file_relations.csv"
copy_file "honeydew_file_relations_intermediate.csv"
copy_file "honeydew_file_relations_all.csv"
copy_file "honeydew_file_relations_all_intermediate.csv"
