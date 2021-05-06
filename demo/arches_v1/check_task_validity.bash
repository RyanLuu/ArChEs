#!/bin/bash
OUTPUT_FILE="verification/$(date +"%m_%d_%Hh_%Mm")_verification.txt"
echo "orthogonal_recursive `echo -e "1 \n orthogonal_recursive \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE &
echo "orthogonal_xaxis     `echo -e "1 \n orthogonal_xaxis     \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "orthogonal_rot90     `echo -e "1 \n orthogonal_rot90     \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "filterColor          `echo -e "1 \n filterColor          \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "filterRecolor        `echo -e "1 \n filterRecolor        \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "filterRecolorFlip    `echo -e "1 \n filterRecolorFlip    \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "multiExample         `echo -e "1 \n multiExample         \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "recolor              `echo -e "1 \n recolor              \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "74dd1130             `echo -e "1 \n 74dd1130             \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "cloneMirror          `echo -e "1 \n cloneMirror          \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "composeComplex       `echo -e "1 \n composeComplex       \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 
echo "identity             `echo -e "1 \n identity             \n\n\n 2 \n 3" | dotnet run | grep "EXPECTED" | wc -l`" >> $OUTPUT_FILE & 


# Source: Stack Overflow -- https://stackoverflow.com/questions/356100/how-to-wait-in-bash-for-several-subprocesses-to-finish-and-return-exit-code-0
FAIL=0
for job in `jobs -p`
do
echo $job
    wait $job || let "FAIL+=1"
done

if [ "$FAIL" == "0" ];
then
echo ""
else
echo "FAILED TO COMPLETE EXECUTION THIS MANY TIMES! ($FAIL)"
fi

sort -o $OUTPUT_FILE $OUTPUT_FILE
echo "$OUTPUT_FILE"
cat $OUTPUT_FILE > verification/latest_output.txt
cat verification/latest_output.txt
