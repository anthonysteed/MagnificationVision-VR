import sys
import json

def parse_log(log_json):
    max_frame = 0
    while True:
        if log_json.get("frame-" + str(max_frame + 1), -1) == -1:
            break        
        max_frame += 1
    print("Highest frame is", max_frame)
    
def main():
    if len(sys.argv) != 2:
        print("Wrong args!")
        sys.exit(1)
    
    with open (sys.argv[1]) as log_file:
        log_json = json.load(log_file)
        parse_log(log_json)
        
        

if __name__ == "__main__":
    main()