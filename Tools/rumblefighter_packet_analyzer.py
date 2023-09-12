import tkinter as tk
from tkinter import scrolledtext

def hex_string_to_bytes(hex_string):
    hex_string = hex_string.replace("0x", "").replace(",", "").replace(" ", "")
    return bytes.fromhex(hex_string)

def update_display():
    hex_input = input_text.get("1.0", "end-1c")
    byte_data = hex_string_to_bytes(hex_input)

    byte_display.config(state=tk.NORMAL)
    byte_display.delete(1.0, tk.END)
    ascii_display.config(state=tk.NORMAL)
    ascii_display.delete(1.0, tk.END)

    position = 0
    for byte in byte_data:
        byte_display.insert(tk.END, f"{byte:02X} ")
        ascii_display.insert(tk.END, f"{chr(byte)}")
        position += 1

    byte_display.config(state=tk.DISABLED)
    ascii_display.config(state=tk.DISABLED)

def on_byte_click(event):
    cursor_index = byte_display.index(tk.CURRENT)
    line, char = map(int, cursor_index.split("."))
    
    # Calculate the position based on the line and character clicked
    position = (line - 1) * 16 + (char // 3)
    position_label.config(text=f"Position: {position}")


# Create the main window
window = tk.Tk()
window.title("Hex Viewer")

# Create a text input field
input_label = tk.Label(window, text="Hex Input:")
input_label.pack()
input_text = scrolledtext.ScrolledText(window, height=5, width=40)
input_text.pack()

# Create a button to update the display
update_button = tk.Button(window, text="Update Display", command=update_display)
update_button.pack()

# Create byte and ASCII displays with a monospaced font
byte_display = scrolledtext.ScrolledText(window, height=10, width=30, font=("Courier New", 12))
byte_display.pack(side=tk.LEFT)
byte_display.config(state=tk.DISABLED)

ascii_display = scrolledtext.ScrolledText(window, height=10, width=30, font=("Courier New", 12))
ascii_display.pack(side=tk.LEFT)
ascii_display.config(state=tk.DISABLED)

# Create a label to display the current position
position_label = tk.Label(window, text="Position: ")
position_label.pack()

# Bind the click event to the byte display
byte_display.bind("<ButtonRelease-1>", on_byte_click)

# Start the main event loop
window.mainloop()
