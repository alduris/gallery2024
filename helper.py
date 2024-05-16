import os
import subprocess

# Read rooms
rooms = {}

with open("World/GR/map_GR.txt", "r", encoding="utf8") as f:
	for line in f:
		split = line[line.find(": ")+2:].split("><")
		rooms[line[:line.find(": ")]] = {"data": split[:-1], "subregion": split[-1].strip()}

with open("World/GR/world_GR.txt", "r", encoding="utf8") as f:
	b = False
	for line in f:
		if line.strip() == "ROOMS":
			b = True
		elif line.strip() == "END ROOMS":
			b = False
		elif b:
			parts = line.strip().split(" : ")
			if parts[0] in rooms:
				rooms[parts[0]]["connections"] = parts[1]

# Utility
COLS = os.get_terminal_size().columns - 2
ROWS = os.get_terminal_size().lines
DEFAULT_MESSAGE = "mv/next/prev/page/exit"
message = DEFAULT_MESSAGE

curr_dir = os.path.dirname(__file__)

def cls():
	os.system("cls" if os.name == "nt" else "clear")

def mv(fr, to):
	# Safety
	if to in rooms:
		message = to + " is already a room. Please pick another name or rename the existing room."
		return
	if not fr in rooms:
		message = fr + " is not a room."
		return
	
	# Get paths
	pathf = "World/GR-Rooms/" + fr
	patht = "World/GR-Rooms/" + to
	
	# Rename files
	subprocess.Popen(["git", "mv", os.path.join(curr_dir, pathf + ".txt"), os.path.join(curr_dir, patht + ".txt")], cwd=curr_dir).wait()
	test = os.path.isfile(pathf + "_settings.txt")
	subprocess.Popen(["git", "mv", os.path.join(curr_dir, "World/GR-Rooms/" + (fr if test else fr.lower()) + "_settings.txt"), os.path.join(curr_dir, "World/GR-Rooms/" + to.lower() + "_settings.txt")], cwd=curr_dir).wait()
	i = 1
	while os.path.isfile(pathf + "_" + str(i) + ".png"):
		subprocess.Popen(["git", "mv", os.path.join(curr_dir, pathf + "_" + str(i) + ".png"), os.path.join(curr_dir, to + "_" + str(i) + ".png")], cwd=curr_dir).wait()
		i += 1
	
	# Move data internally
	rooms[to] = rooms[fr]
	del rooms[fr]

	# Save
	order = sorted(rooms.keys(), key=str.casefold)
	with open("World/GR/map_GR.txt", "w", encoding="utf8") as f:
		b = False
		for key in order:
			r = rooms[key]
			f.write(key + ": " + "><".join(r["data"]) + "><" + r["subregion"])
			if b:
				f.write("\n")
			else:
				b = True

	with open("World/GR/world_GR.txt", "w", encoding="utf8") as f:
		f.write("ROOMS\n\n")
		for key in order:
			if key.lower().startswith("offscreen"): continue
			r = rooms[key]
			f.write(key + " : " + r["connections"] + "\n")
		f.write("END ROOMS\nCREATURES\n\nEND CREATURES\n")

# Main loop
page = 1
while True:
	print("PAGE " + str(page) + " | " + message)
	print("=" * COLS)
	order = sorted(rooms.keys(), key=str.casefold)
	maxlen = 3
	for item in order:
		l = len(item) + len(rooms[item]["subregion"]) + 5
		if l > maxlen:
			maxlen = l
	numcols = int(COLS / maxlen)
	
	i = 0
	j = 1
	k = 1
	colwidth = int(COLS / numcols)
	for item in order:
		if i + maxlen > COLS:
			print()
			i = 0
			j += 1
			if j > ROWS - 5:
				k += 1
				if k > page:
					break
				j = 0
		if k == page:
			l = len(item) + len(rooms[item]["subregion"])
			print(item + " (" + rooms[item]["subregion"] + ")", end=(" " * (maxlen - l)))
		i += maxlen
	
	for i in range(j, ROWS - 4):
		print()
	print("=" * COLS)
	
	inp = input("Action: ")
	cls()
	
	if inp.startswith("mv"):
		split = inp.split(" ")
		if len(split) != 3:
			message = "Invalid usage of mv! Syntax: mv [from] [to]"
		mv(split[1], split[2])
		message = DEFAULT_MESSAGE
	elif inp == "exit":
		exit()
	elif inp == "next" or inp == "n":
		page += 1
		message = DEFAULT_MESSAGE
	elif inp == "prev" or inp == "p":
		page -= 1
		message = DEFAULT_MESSAGE
	elif inp.startswith("page"):
		if " " not in inp:
			message = "Invalid usage of page! Syntax: page [number]"
			break
		page = int(inp.split(" ")[1])
	else:
		message = "Invalid command!"
