        # Define the path to focus on (Portal/src/Datahub.Portal)
        target_path="Portal/src/Datahub.Portal"

        # Load the English and French JSON files into variables
        english_json=$(cat "$target_path/i18n/localization.json" | jq -r '.')
        french_json=$(cat "$target_path/i18n/localization.fr.json" | jq -r '.')

        # Create an array to hold file paths with matching Localizer patterns
        matching_files=()

        # Define the regex pattern to match @Localizer[""] or Localizer[""]
        pattern='@Localizer\["|Localizer\["'

        # Find all .cs and .razor files and search for the Localizer pattern
        while IFS= read -r file; do
            # Search for the regex pattern in the file
            if grep -qP "$pattern" "$file"; then
                echo "Pattern found in file: $file"
                matching_files+=("$file")
            fi
        done < <(find "$target_path" -type f \( -name "*.cs" -o -name "*.razor" \))

        # Now, we will search for missing translations in the matched files
        error_found=0  # Variable to track if an error is found

        # Iterate over the files with Localizer patterns
        for file in "${matching_files[@]}"; do
            echo "Checking translations in file: $file"

            # Extract Localizer keys from the file (line-delimited)
            keys=$(grep -Po '(?<=Localizer\["|@Localizer\[").+?(?="\])' "$file" | sort | uniq)
            
            # Process each key line by line
            while IFS= read -r key; do
                # Shorten the key to the first 5 words inline using awk
                shortened_key=$(echo "$key" | awk '{if (NF>5) {for(i=1;i<=5;i++) printf $i " "; print "..."} else print $0}')

                # Check if the key exists in the English JSON file
                if ! echo "$english_json" | jq -e --arg key "$key" '.[$key]' > /dev/null; then
                    echo "Error: Missing English translation for key '$shortened_key' in file '$file'"
                    error_found=1
                fi

                # Check if the key exists in the French JSON file
                if ! echo "$french_json" | jq -e --arg key "$key" '.[$key]' > /dev/null; then
                    echo "Error: Missing French translation for key '$shortened_key' in file '$file'"
                    error_found=1
                fi
            done <<< "$keys"
        done

        # If any error is found, exit with an error code; otherwise, exit cleanly
        if [ "$error_found" -eq 1 ]; then
            echo "Missing translations detected!"
            exit 1
        else
            echo "All translations found!"
            exit 0