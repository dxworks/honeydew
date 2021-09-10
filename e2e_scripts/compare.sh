if test $# -ne 2
then 
	echo "Usage $0 <source_folder> <destination_folder>"; exit 1
fi

java -jar ./filecomparer.jar csv "$1/honeydew.csv" "$2/honeydew.csv"
java -jar ./filecomparer.jar csv "$1/honeydew_intermediate.csv" "$2/honeydew_intermediate.csv"
java -jar ./filecomparer.jar json "$1/honeydew.json" "$2/honeydew.json"
java -jar ./filecomparer.jar json "$1/honeydew_intermediate.json" "$2/honeydew_intermediate.json"
java -jar ./filecomparer.jar json "$1/honeydew_cyclomatic.json" "$2/honeydew_cyclomatic.json"
java -jar ./filecomparer.jar json "$1/honeydew_cyclomatic_intermediate.json" "$2/honeydew_cyclomatic_intermediate.json"
java -jar ./filecomparer.jar json "$1/honeydew_namespaces.json" "$2/honeydew_namespaces.json"
java -jar ./filecomparer.jar json "$1/honeydew_file_relations.csv" "$2/honeydew_file_relations.csv"
java -jar ./filecomparer.jar json "$1/honeydew_file_relations_intermediate.csv" "$2/honeydew_file_relations_intermediate.csv"
