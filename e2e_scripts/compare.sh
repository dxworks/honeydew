if test $# -ne 2
then 
	echo "Usage $0 <source_folder> <destination_folder>"; exit 1
fi

for file in `ls $1`
do
	if ! test -f $1/$file
		then continue
	fi	
	
	if ! cmp -s "$1/$file" "$2/$file"
	then echo "Not Equal $1/$file $2/$file"; echo `cat $1/$file`; echo `cat $2/$file`;exit 2
	fi
done
