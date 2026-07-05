import math
from PIL import Image, ImageDraw, ImageFilter, ImageFont

def create_hyperboost_icon():
    size = 256
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 1. Outer Glowing Cyber Circle (Cyan & Green gradient effect)
    for r in range(120, 90, -1):
        alpha = int(255 * ((r - 90) / 30.0) * 0.4)
        draw.ellipse([(128 - r, 128 - r), (128 + r, 128 + r)], fill=None, outline=(0, 242, 255, alpha), width=2)

    # 2. Base Dark Shield/Circle
    draw.ellipse([(20, 20), (236, 236)], fill=(11, 15, 25, 250), outline=(0, 210, 255, 255), width=4)
    draw.ellipse([(28, 28), (228, 228)], fill=None, outline=(0, 242, 128, 180), width=2)

    # 3. Inner Cybernetic Grid / Speed lines
    for i in range(40, 220, 30):
        draw.line([(i, 40), (i, 216)], fill=(0, 210, 255, 25), width=1)
        draw.line([(40, i), (216, i)], fill=(0, 210, 255, 25), width=1)

    # 4. Neon Lightning Bolt & Rocket Shield (HyperBoost)
    # Lightning coordinates
    bolt = [
        (135, 45),
        (85, 130),
        (120, 130),
        (105, 210),
        (175, 115),
        (135, 115)
    ]
    
    # Draw glow for bolt
    bolt_img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    bolt_draw = ImageDraw.Draw(bolt_img)
    bolt_draw.polygon(bolt, fill=(0, 242, 128, 200), outline=(255, 255, 255, 255))
    bolt_glow = bolt_img.filter(ImageFilter.GaussianBlur(5))
    img.alpha_composite(bolt_glow)

    # Draw solid bolt
    draw.polygon(bolt, fill=(0, 255, 160, 255), outline=(255, 255, 255, 255), width=2)

    # 5. Add "5G" and "PRO" text accents if possible, or speed arcs
    draw.arc([(50, 50), (206, 206)], start=-45, end=45, fill=(255, 0, 127, 255), width=6)
    draw.arc([(50, 50), (206, 206)], start=135, end=225, fill=(0, 210, 255, 255), width=6)

    # Save to multiple sizes for Windows ICO
    sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
    icons = [img.resize(s, Image.Resampling.LANCZOS) for s in sizes]
    
    img.save("app.ico", format="ICO", sizes=sizes)
    print("Successfully generated HyperBoost app.ico!")

if __name__ == "__main__":
    create_hyperboost_icon()
