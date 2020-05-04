import sys
import json
import os
import numpy as np

glob_item_positions = [
    { # permutation 1
        "BROWN_CHEESE": (1.767, 0.4242516, -21.158),
        "BROKEN_BOTTLE": (3.935, 0.807, -9.871),
        "HEADLESS_FISH": (7.363, 0.809, -40.293),
        "MARGARINE": (3.2713, 0.459, -35.316),
        "PINEAPPLE_PIZZA": (-0.498, 0.538, -27.656)
    },
    { # permutation 2
        "PINEAPPLE_PIZZA": (-8.399718, 0.54, -44.4046),
        "HEADLESS_FISH": (6.901, 0.409, -36.793),
        "BROKEN_BOTTLE": (-5.976, 1.19, -26.372),
        "BROWN_CHEESE": (7.058, 0.797, -15.881),
        "MARGARINE": (1.587, 0.275, -37.903)
    },
    { # permutation 3
        "PINEAPPLE_PIZZA": (-4.396, 1.074, -8.886),
        "BROWN_CHEESE": (6.732431, 0.4201571, -32.80882),
        "HEADLESS_FISH": (8.204, 0.409, -16.535),
        "MARGARINE": (1.618, 0.282, -19.763),
        "BROKEN_BOTTLE": (-1.489, 0.157, -31.217)
    }
]

def get_max_frame(log_json):
    max_frame = 0
    while True:
        if not log_json.get("frame-" + str(max_frame + 1)):
            break        
        max_frame += 1
    return max_frame
    

def parse_log(log_json, iteration):
    max_frame = get_max_frame(log_json)
    start_time = get_start_time(log_json)
    out_str = ""
    #out_str += "Highest frame is %d\n" % max_frame
    #print("Completion time is", get_completion_time(log_json, max_frame, start_time))
    #out_str += "Mag time: %f\n" % get_mag_time(log_json, start_time, max_frame)
    #out_str += "Number of teleports: %d\n" % get_num_teleports(log_json, start_time)
    
    #out_str += "Distance covered: %f\n" % get_distance_covered(log_json, start_time)
    #out_str += "Number of item misses: %d\n" % get_missed_items(log_json, max_frame)
    #out_str += "Num. finds through MagRect: %d\n" % get_mag_rect_finds(log_json)
    #out_str += "Time with raised hands: %f\n" % get_hands_raised_time(log_json, start_time, max_frame)
    #out_str += "Mean item-finding distance: %f\n" % get_mean_item_find_distance(log_json, iteration)

    return out_str

def get_distance_covered(log_json, start_time):
    dist = 0.0
    buffered_position = []
    last_buffered = []
    mod = 0
    for key in log_json:
        frame = log_json[key]
        if frame == 1 or frame["time"] <= start_time:
            continue
        pos = frame["Player"]["GlobalHeadPos"]
        pos[1] = 0.0 # ignore y-movement

        if len(buffered_position) > 0:        
            buffered_position = np.divide(np.add(pos, buffered_position), 2.0)
        else:
            buffered_position = pos

        mod += 1
        if mod % 90 == 0:
            if len(last_buffered) > 0:
                dist += np.linalg.norm(np.subtract(last_buffered, buffered_position))
                last_buffered = buffered_position
                buffered_position = []
            else:
                last_buffered = buffered_position
    return dist


def get_start_time(log_json):
    # Check whether tutorial was done; if so start from there
    start_time = 0.0
    for key in log_json:
        frame = log_json[key]
        gm = (frame != 1) and frame.get("GameManager")
        if gm and gm.get("tutorialComplete") == "True":
            start_time = frame["time"]
            break
    return start_time

def get_completion_time(log_json, max_frame, start_time):
    # Find GameManager:gameOver (searching backwards)
    for i in range(max_frame, 0, -1):
        frame = log_json["frame-%d" % i]
        gm = frame.get("GameManager")
        game_over = gm and gm.get("gameOver")            
        if game_over == "True":
            return frame["time"] - start_time
    print("ERROR - couldn't find completion time")
    
def get_hands_raised_time(log_json, start_time, max_frame):
    raised_time = 0.0
    last_lowered_timestamp = 0.0
    hands_raised = False

    # Need to go in chronological order
    for i in range(1, max_frame):
        frame = log_json["frame-%d" % i]
        controller = frame.get("Controller")
        if frame["time"] <= start_time or not controller:
            continue
        height = frame["Player"]["GlobalHeadPos"][1]

        l_pos = controller.get("LPos")
        r_pos = controller.get("RPos")

        if l_pos is None or r_pos is None:
            continue
        
        right_hand_height = r_pos[1]
        left_hand_height = l_pos[1]

        threshold = (2.0 / 3.0) * height
        if left_hand_height > threshold and right_hand_height > threshold:
            hands_raised = True
        else:
            if hands_raised:
                hands_raised = False
                if last_lowered_timestamp > 0:
                    raised_time += frame["time"] - last_lowered_timestamp
            last_lowered_timestamp = frame["time"]
    return raised_time

def get_mean_item_find_distance(log_json, iteration):
    mean_dist = 0.0
    
    for key in log_json:
        frame = log_json[key]
        items = (frame != 1) and frame.get("Items")
        if not items:
            continue
        
        for item_key in items:
            if "_discovered" in item_key and items[item_key] == "True":
                item_name = item_key[0:item_key.index("_discovered")]
                item_pos = glob_item_positions[iteration - 1][item_name]
                player_pos = frame["Player"]["GlobalHeadPos"]
                to_item = np.subtract(item_pos, player_pos)
                mean_dist += np.linalg.norm(to_item)    
                break
    return mean_dist / 5.0

def get_mag_time(log_json, start_time, max_frame):
    prev_frame = None
    mag_time = 0.0
    # Need to go in chronological order
    for i in range(1, max_frame):
        frame = log_json["frame-%d" % i]
        mag_rect = frame.get("MagRect")
        active = mag_rect and mag_rect.get("active")
        if active == "True" and prev_frame:
            mag_time += frame["time"] - prev_frame["time"]
        prev_frame = frame
    return mag_time - start_time


def get_num_teleports(log_json, start_time):
    num_teleports = 0
    for key in log_json:
        frame = log_json[key]
        gt = (frame != 1) and frame.get("GazeTeleport")
        teleport = gt and gt.get("isTeleporting")
        if teleport == "True" and frame["time"] > start_time:
            num_teleports += 1
    return num_teleports

def get_missed_items(log_json, max_frame):
    num_misses = 0
    item_visible = False
    last_vis_item_key = None

    # Make sure we go in chronological order
    for i in range(1, max_frame):
        frame = log_json["frame-%d" % i]
        items = frame.get("Items")
        if items:
            # Check if item found this frame
            for item_key in items:
                if "_discovered" in item_key and items[item_key] == "True":
                    item_visible = False
                    break

            # If not, check whether item still visible
            if item_visible:
                if items.get(last_vis_item_key) != "True": # no longer visible
                    num_misses += 1
            
            for item_key in items:
                if "Fov" in item_key and items[item_key] == "True":
                    item_visible = True
                    last_vis_item_key = item_key
                    break
        else:
            if item_visible:
                num_misses += 1
            item_visible = False
    return num_misses

def get_iteration_no(filename):
    l_paren = filename.index('(') + 1
    r_paren = filename.index(')')

    iteration_no = int(filename[l_paren:r_paren])
    assert iteration_no > 0 and iteration_no < 4
    return iteration_no

# i.e. num. item finds through MagRect
def get_mag_rect_finds(log_json):
    num_finds = 0
    for key in log_json:
        frame = log_json[key]
        items = (frame != 1) and frame.get("Items")
        if items:
            for item_key in items:
                item_name = None
                if "_inRectFov" in item_key and items[item_key] == "True":
                    item_name = item_key[0:item_key.index("_inRectFov")]
                    if items.get(item_name + "_discovered") == "True":
                        num_finds += 1
                    break
    return num_finds


def main():
    if "-r" in sys.argv:
        assert len(sys.argv) == 3

        with open("./logout.txt", 'w') as out_file:
            folder = sys.argv[2]
            for root, _, files in os.walk(folder):
                for file in files:
                    filename = root + '/' + file 
                    print("Parsing", filename)
                    with open(filename) as log_file:
                        log_json = json.load(log_file)
                        out_file.write("\n" + filename + "\n")

                        iteration = get_iteration_no(filename)
                        out_file.write(parse_log(log_json, iteration))
    else:
        assert len(sys.argv) == 2

        with open(sys.argv[1]) as log_file:
            log_json = json.load(log_file)
            iteration = get_iteration_no(sys.argv[1])
            print(parse_log(log_json, iteration))
    print("DONE!")
        
        

if __name__ == "__main__":
    main()