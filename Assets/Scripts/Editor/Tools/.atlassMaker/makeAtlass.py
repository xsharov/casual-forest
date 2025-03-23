from PIL import Image
import glob
import math

cell_size = 512
png_files = glob.glob("input/*.png")
png_files.sort()

num_images = len(png_files)
if num_images == 0:
    print("No PNG files found in 'input' folder.")
    exit()

grid_size = math.ceil(math.sqrt(num_images))
total_cells = grid_size * grid_size

images = []
for file in png_files:
    try:
        img = Image.open(file)
        if img.size != (cell_size, cell_size):
            img = img.resize((cell_size, cell_size))
        images.append(img)
    except Exception as e:
        print(f"Error opening file {file}: {e}")

while len(images) < total_cells:
    images.append(Image.new('RGBA', (cell_size, cell_size), (0, 0, 0, 0)))

atlas = Image.new('RGBA', (grid_size * cell_size, grid_size * cell_size))

for index, img in enumerate(images):
    row = index // grid_size
    col = index % grid_size
    x = col * cell_size
    y = row * cell_size
    atlas.paste(img, (x, y))

atlas.save("atlas.png")
print(f"Atlas created with grid {grid_size}x{grid_size} and saved as atlas.png")
