/*
 * Decryption utility for encrypted files
 * Maps encrypted bytes (index) to decrypted bytes (value)
 * Generated from final-map.txt
 */

#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <glob.h>
#include <errno.h>
#include <sys/stat.h>

uint8_t decrypt_map[256] = {
    0x1d, 0x3d, 0x5d, 0x7d, 0x9d, 0xbd, 0xdd, 0xfd,
    0x96, 0xb6, 0xd6, 0xf6, 0x16, 0x36, 0x56, 0x76,
    0x68, 0x88, 0xa8, 0xc8, 0xe8, 0x08, 0x28, 0x48,
    0x03, 0x23, 0x43, 0x63, 0x83, 0xa3, 0xc3, 0xe3,
    0x21, 0x41, 0x61, 0x81, 0xa1, 0xc1, 0xe1, 0x01,
    0x9a, 0xba, 0xda, 0xfa, 0x1a, 0x3a, 0x5a, 0x7a,
    0x6c, 0x8c, 0xac, 0xcc, 0xec, 0x0c, 0x2c, 0x4c,
    0x07, 0x27, 0x47, 0x67, 0x87, 0xa7, 0xc7, 0xe7,
    0x25, 0x45, 0x65, 0x85, 0xa5, 0xc5, 0xe5, 0x05,
    0x9e, 0xbe, 0xde, 0xfe, 0x1e, 0x3e, 0x5e, 0x7e,
    0x70, 0x90, 0xb0, 0xd0, 0xf0, 0x10, 0x30, 0x50,
    0x0b, 0x2b, 0x4b, 0x6b, 0x8b, 0xab, 0xcb, 0xeb,
    0x29, 0x49, 0x69, 0x89, 0xa9, 0xc9, 0xe9, 0x09,
    0xa2, 0xc2, 0xe2, 0x02, 0x22, 0x42, 0x62, 0x82,
    0x74, 0x94, 0xb4, 0xd4, 0xf4, 0x14, 0x34, 0x54,
    0x0f, 0x2f, 0x4f, 0x6f, 0x8f, 0xaf, 0xcf, 0xef,
    0x2d, 0x4d, 0x6d, 0x8d, 0xad, 0xcd, 0xed, 0x0d,
    0xa6, 0xc6, 0xe6, 0x06, 0x26, 0x46, 0x66, 0x86,
    0x78, 0x98, 0xb8, 0xd8, 0xf8, 0x18, 0x38, 0x58,
    0x13, 0x33, 0x53, 0x73, 0x93, 0xb3, 0xd3, 0xf3,
    0x31, 0x51, 0x71, 0x91, 0xb1, 0xd1, 0xf1, 0x11,
    0xaa, 0xca, 0xea, 0x0a, 0x2a, 0x4a, 0x6a, 0x8a,
    0x7c, 0x9c, 0xbc, 0xdc, 0xfc, 0x1c, 0x3c, 0x5c,
    0x17, 0x37, 0x57, 0x77, 0x97, 0xb7, 0xd7, 0xf7,
    0x35, 0x55, 0x75, 0x95, 0xb5, 0xd5, 0xf5, 0x15,
    0xae, 0xce, 0xee, 0x0e, 0x2e, 0x4e, 0x6e, 0x8e,
    0x80, 0xa0, 0xc0, 0xe0, 0x00, 0x20, 0x40, 0x60,
    0x1b, 0x3b, 0x5b, 0x7b, 0x9b, 0xbb, 0xdb, 0xfb,
    0x39, 0x59, 0x79, 0x99, 0xb9, 0xd9, 0xf9, 0x19,
    0xb2, 0xd2, 0xf2, 0x12, 0x32, 0x52, 0x72, 0x92,
    0x84, 0xa4, 0xc4, 0xe4, 0x04, 0x24, 0x44, 0x64,
    0x1f, 0x3f, 0x5f, 0x7f, 0x9f, 0xbf, 0xdf, 0xff
};

// Function to determine output filename based on input filename
char* get_output_filename(const char* input_filename) {
    const char* dot = strrchr(input_filename, '.');
    if (!dot || strcasecmp(dot, ".bin") != 0) {
        // No extension or not a .bin file, won't process
        return NULL;
    }
    
    // For .bin files, append .dec to create .bin.dec
    size_t len = strlen(input_filename);
    char* output = malloc(len + 5);
    strcpy(output, input_filename);
    strcat(output, ".dec");
    
    return output;
}

// Function to decrypt a single file
int decrypt_file(const char* input_filename) {
    // Get output filename
    char* output_filename = get_output_filename(input_filename);
    if (!output_filename) {
        printf("Skipped: %s (unsupported extension)\n", input_filename);
        return 0;  // Return success, but skip the file
    }
    
    FILE* input_file = fopen(input_filename, "rb");
    if (!input_file) {
        fprintf(stderr, "Error: Cannot open input file '%s': %s\n", 
                input_filename, strerror(errno));
        free(output_filename);
        return 1;
    }
    
    
    // Open output file
    FILE* output_file = fopen(output_filename, "wb");
    if (!output_file) {
        fprintf(stderr, "Error: Cannot create output file '%s': %s\n", 
                output_filename, strerror(errno));
        fclose(input_file);
        free(output_filename);
        return 1;
    }
    
    // Process file byte by byte
    int byte;
    long bytes_processed = 0;
    
    while ((byte = fgetc(input_file)) != EOF) {
        uint8_t decrypted_byte = decrypt_map[(uint8_t)byte];
        
        if (fputc(decrypted_byte, output_file) == EOF) {
            fprintf(stderr, "Error: Failed to write to output file '%s': %s\n", 
                    output_filename, strerror(errno));
            fclose(input_file);
            fclose(output_file);
            free(output_filename);
            return 1;
        }
        
        bytes_processed++;
    }
    
    // Check for read errors
    if (ferror(input_file)) {
        fprintf(stderr, "Error: Failed to read from input file '%s': %s\n", 
                input_filename, strerror(errno));
        fclose(input_file);
        fclose(output_file);
        free(output_filename);
        return 1;
    }
    
    fclose(input_file);
    fclose(output_file);
    
    if (bytes_processed == 0) {
        fprintf(stderr, "Warning: File '%s' is empty\n", input_filename);
    }
    
    printf("Decrypted: %s -> %s (%ld bytes)\n", 
           input_filename, output_filename, bytes_processed);
    
    free(output_filename);
    return 0;
}

// Function to process files from glob pattern
int process_glob_pattern(const char* pattern) {
    glob_t glob_result;
    int glob_status = glob(pattern, GLOB_TILDE, NULL, &glob_result);
    
    if (glob_status != 0) {
        if (glob_status == GLOB_NOMATCH) {
            fprintf(stderr, "No files match pattern: %s\n", pattern);
        } else {
            fprintf(stderr, "Error processing glob pattern: %s\n", pattern);
        }
        return 1;
    }
    
    int errors = 0;
    for (size_t i = 0; i < glob_result.gl_pathc; i++) {
        if (decrypt_file(glob_result.gl_pathv[i]) != 0) {
            errors++;
        }
    }
    
    globfree(&glob_result);
    return errors;
}

void print_usage(const char* program_name) {
    printf("Usage: %s <file1> [file2] [file3] ...\n", program_name);
    printf("       %s <glob_pattern>\n", program_name);
    printf("\nDecrypts files using the built-in mapping table.\n");
    printf("Only files with .bin extensions are processed.\n");
    printf("Other files are skipped.\n");
    printf("\nOutput filename rules:\n");
    printf("  *.bin -> *.bin.dec\n");
    printf("\nExamples:\n");
    printf("  %s file1.bin file2.bin\n", program_name);
    printf("  %s \"*.bin\"\n", program_name);
    printf("  %s \"data/*.bin\"\n", program_name);
}

int main(int argc, char* argv[]) {
    if (argc < 2) {
        print_usage(argv[0]);
        return 1;
    }
    
    int total_errors = 0;
    
    for (int i = 1; i < argc; i++) {
        // Check if argument contains glob characters
        if (strchr(argv[i], '*') || strchr(argv[i], '?') || strchr(argv[i], '[')) {
            total_errors += process_glob_pattern(argv[i]);
        } else {
            if (decrypt_file(argv[i]) != 0) {
                total_errors++;
            }
        }
    }
    
    if (total_errors > 0) {
        fprintf(stderr, "\nCompleted with %d error(s)\n", total_errors);
        return 1;
    }
    
    printf("\nAll files processed successfully\n");
    return 0;
}
