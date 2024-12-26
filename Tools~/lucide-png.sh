#!/bin/bash

# dos2unix lucide-png.sh

RELEASE_VERSION="0.469.0"
DOWNLOAD_URL="https://github.com/lucide-icons/lucide/releases/download/${RELEASE_VERSION}/lucide-icons-${RELEASE_VERSION}.zip"
ZIP_FILENAME="lucide-icons-${RELEASE_VERSION}.zip"
EXTRACTION_DIR="../Runtime/Assets/Lucide"
PNG_EXPORT_SIZE=1024

sudo apt-get update
sudo apt-get install -y wget curl unzip inkscape imagemagick

echo "Downloading ZIP file from: $DOWNLOAD_URL"
wget -O "$ZIP_FILENAME" "$DOWNLOAD_URL"
wget_status=$?
if [ $wget_status -ne 0 ]; then
    echo "Error: Failed to download the ZIP file. Please check your internet connection and the URL."
    exit $wget_status
fi

echo "Extracting ZIP file: $ZIP_FILENAME"
unzip -o "$ZIP_FILENAME" -d "$EXTRACTION_DIR"
unzip_status=$?
if [ $unzip_status -ne 0 ]; then
    echo "Error: Failed to extract the ZIP file. Please ensure the ZIP file is not corrupted."
    exit $unzip_status
fi

echo "Organizing files..."
find "$EXTRACTION_DIR" -type f ! -iname "*.svg" ! -iname "*.png" -delete
find "$EXTRACTION_DIR" -type f -exec mv {} "$EXTRACTION_DIR/" \;
find "$EXTRACTION_DIR" -type d -empty -delete
rm $ZIP_FILENAME

echo "Searching for SVG files..."
mapfile -t svg_files < <(find "$EXTRACTION_DIR" -type f -iname "*.svg")

if [ ${#svg_files[@]} -eq 0 ]; then
    echo "Error: No SVG files found in the extracted directory."
    exit 1
fi

echo "Number of SVG files found: ${#svg_files[@]}"

echo "Converting SVG files to PNG..."
for svg_file in "${svg_files[@]}"; do
    base_name=$(basename "$svg_file" .svg)
    output_png="$EXTRACTION_DIR/${base_name}.png"

    inkscape "$svg_file" --export-filename="$output_png" --export-width="$PNG_EXPORT_SIZE" --export-height="$PNG_EXPORT_SIZE" >/dev/null 2>&1
    inkscape_status=$?
    if [ $inkscape_status -ne 0 ]; then
        echo "Failed to convert: $svg_file"
        continue
    fi

    convert "$output_png" -fill white -colorize 100% "$output_png" >/dev/null 2>&1
    convert_status=$?
    if [ $convert_status -ne 0 ]; then
        echo "Failed to colorize: $output_png"
        continue
    fi

    echo "Successfully converted: $svg_file → $output_png"
    rm "$svg_file"
done

echo "All conversions complete."
