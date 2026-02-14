#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <glob.h>
#include <errno.h>
#include <sys/stat.h>

static uint8_t bit_reverse_map[256];


// Reverse the bits in a byte.
// eg 0x87 is 0b10000111, reverse is 0b11100001 which is 0xe1
uint8_t reverse_byte(uint8_t byte)
{
	uint8_t result = 0;
	for (int i = 0; i < 8; i++){
		if (((byte >> i) & 1) == 1){  // a bit set in position i
			result += (1 << (7 - i)); // sets a bit in position 7-i
		}
	}
	return result;
}


// Function to determine output filename based on input filename
char* get_output_filename(const char* input_filename) {
    const char* dot = strrchr(input_filename, '.');
    if (!dot) {
        // No extension, won't process
        return NULL;
    }
    
    size_t base_len = dot - input_filename;
    char* output = malloc(base_len + 5);
    strncpy(output, input_filename, base_len);
    output[base_len] = '\0';
    
    if (strcasecmp(dot, ".exdf") == 0) {
        strcat(output, ".xml");
    } else if (strcasecmp(dot, ".mff") == 0) {
        strcat(output, ".cab");
    } else {
        // For other extensions, return NULL, won't process
        free(output);
        return NULL;
    }
    
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
        uint8_t decrypted_byte = bit_reverse_map[(uint8_t)byte ^ 0x55];
        
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
    printf("Only files with .exdf or .mff extensions are processed.\n");
    printf("Other files are skipped.\n");
    printf("\nOutput filename rules:\n");
    printf("  *.exdf -> *.xml\n");
    printf("  *.mff  -> *.cab\n");
    printf("\nExamples:\n");
    printf("  %s file1.exdf file2.mff\n", program_name);
    printf("  %s \"*.exdf\"\n", program_name);
    printf("  %s \"data/*.mff\"\n", program_name);
}


int main(int argc, char* argv[]) {
    if (argc < 2) {
        print_usage(argv[0]);
        return 1;
    }
    
    int total_errors = 0;

	for (int i = 0; i < 256; i++){
		bit_reverse_map[i] = reverse_byte(i);
	}
    
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
