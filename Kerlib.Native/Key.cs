namespace Kerlib.Native;

public sealed class Key
{
    public int VirtualCode { get; }
    public string Name { get; }

    private Key(int virtualCode, string name)
    {
        VirtualCode = virtualCode;
        Name = name;
    }

    public static readonly Key A = new(0x41, "A");
    public static readonly Key B = new(0x42, "B");
    public static readonly Key C = new(0x43, "C");
    public static readonly Key D = new(0x44, "D");
    public static readonly Key E = new(0x45, "E");
    public static readonly Key F = new(0x46, "F");
    public static readonly Key G = new(0x47, "G");
    public static readonly Key H = new(0x48, "H");
    public static readonly Key I = new(0x49, "I");
    public static readonly Key J = new(0x4A, "J");
    public static readonly Key K = new(0x4B, "K");
    public static readonly Key L = new(0x4C, "L");
    public static readonly Key M = new(0x4D, "M");
    public static readonly Key N = new(0x4E, "N");
    public static readonly Key O = new(0x4F, "O");
    public static readonly Key P = new(0x50, "P");
    public static readonly Key Q = new(0x51, "Q");
    public static readonly Key R = new(0x52, "R");
    public static readonly Key S = new(0x53, "S");
    public static readonly Key T = new(0x54, "T");
    public static readonly Key U = new(0x55, "U");
    public static readonly Key V = new(0x56, "V");
    public static readonly Key W = new(0x57, "W");
    public static readonly Key X = new(0x58, "X");
    public static readonly Key Y = new(0x59, "Y");
    public static readonly Key Z = new(0x5A, "Z");

    public static readonly Key D0 = new(0x30, "0");
    public static readonly Key D1 = new(0x31, "1");
    public static readonly Key D2 = new(0x32, "2");
    public static readonly Key D3 = new(0x33, "3");
    public static readonly Key D4 = new(0x34, "4");
    public static readonly Key D5 = new(0x35, "5");
    public static readonly Key D6 = new(0x36, "6");
    public static readonly Key D7 = new(0x37, "7");
    public static readonly Key D8 = new(0x38, "8");
    public static readonly Key D9 = new(0x39, "9");

    public static readonly Key F1 = new(0x70, "F1");
    public static readonly Key F2 = new(0x71, "F2");
    public static readonly Key F3 = new(0x72, "F3");
    public static readonly Key F4 = new(0x73, "F4");
    public static readonly Key F5 = new(0x74, "F5");
    public static readonly Key F6 = new(0x75, "F6");
    public static readonly Key F7 = new(0x76, "F7");
    public static readonly Key F8 = new(0x77, "F8");
    public static readonly Key F9 = new(0x78, "F9");
    public static readonly Key F10 = new(0x79, "F10");
    public static readonly Key F11 = new(0x7A, "F11");
    public static readonly Key F12 = new(0x7B, "F12");

    public static readonly Key Escape = new(0x1B, "Escape");
    public static readonly Key Space = new(0x20, "Space");
    public static readonly Key Enter = new(0x0D, "Enter");
    public static readonly Key Backspace = new(0x08, "Backspace");
    public static readonly Key Tab = new(0x09, "Tab");
    public static readonly Key Shift = new(0x10, "Shift");
    public static readonly Key Control = new(0x11, "Control");
    public static readonly Key Alt = new(0x12, "Alt");
    public static readonly Key CapsLock = new(0x14, "CapsLock");
    public static readonly Key Windows = new(0x5B, "Windows");
    public static readonly Key Menu = new(0x5D, "Menu");

    public static readonly Key Left = new(0x25, "Left");
    public static readonly Key Up = new(0x26, "Up");
    public static readonly Key Right = new(0x27, "Right");
    public static readonly Key Down = new(0x28, "Down");
    public static readonly Key Home = new(0x24, "Home");
    public static readonly Key End = new(0x23, "End");
    public static readonly Key PageUp = new(0x21, "PageUp");
    public static readonly Key PageDown = new(0x22, "PageDown");
    public static readonly Key Insert = new(0x2D, "Insert");
    public static readonly Key Delete = new(0x2E, "Delete");

    public static readonly Key NumPad0 = new(0x60, "NumPad0");
    public static readonly Key NumPad1 = new(0x61, "NumPad1");
    public static readonly Key NumPad2 = new(0x62, "NumPad2");
    public static readonly Key NumPad3 = new(0x63, "NumPad3");
    public static readonly Key NumPad4 = new(0x64, "NumPad4");
    public static readonly Key NumPad5 = new(0x65, "NumPad5");
    public static readonly Key NumPad6 = new(0x66, "NumPad6");
    public static readonly Key NumPad7 = new(0x67, "NumPad7");
    public static readonly Key NumPad8 = new(0x68, "NumPad8");
    public static readonly Key NumPad9 = new(0x69, "NumPad9");
    public static readonly Key Multiply = new(0x6A, "Multiply");
    public static readonly Key Add = new(0x6B, "Add");
    public static readonly Key Subtract = new(0x6D, "Subtract");
    public static readonly Key Decimal = new(0x6E, "Decimal");
    public static readonly Key Divide = new(0x6F, "Divide");

    public static readonly Key Oem1 = new(0xBA, "OEM1"); // ;:
    public static readonly Key OemPlus = new(0xBB, "OEMPlus"); // +=
    public static readonly Key OemComma = new(0xBC, "OEMComma"); // ,<
    public static readonly Key OemMinus = new(0xBD, "OEMMinus"); // -_
    public static readonly Key OemPeriod = new(0xBE, "OEMPeriod"); // .>
    public static readonly Key Oem2 = new(0xBF, "OEM2"); // /?
    public static readonly Key Oem3 = new(0xC0, "OEM3"); // `~
    public static readonly Key Oem4 = new(0xDB, "OEM4"); // [{
    public static readonly Key Oem5 = new(0xDC, "OEM5"); // \|
    public static readonly Key Oem6 = new(0xDD, "OEM6"); // ]}
    public static readonly Key Oem7 = new(0xDE, "OEM7"); // '"

    private static readonly Key?[] KeyMap = new Key?[256];
    private static readonly Dictionary<int, Key> UnknownKeys = new();

    static Key()
    {
        Register(A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
                 D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
                 F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
                 Escape, Space, Enter, Backspace, Tab, Shift, Control, Alt, CapsLock, Windows, Menu,
                 Left, Up, Right, Down, Home, End, PageUp, PageDown, Insert, Delete,
                 NumPad0, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
                 Multiply, Add, Subtract, Decimal, Divide,
                 Oem1, OemPlus, OemComma, OemMinus, OemPeriod, Oem2, Oem3, Oem4, Oem5, Oem6, Oem7);
    }

    private static void Register(params Key[] keys)
    {
        foreach (var key in keys)
        {
            if (key.VirtualCode >= 0 && key.VirtualCode < KeyMap.Length)
                KeyMap[key.VirtualCode] = key;
        }
    }

    public static Key FromVirtualCode(int virtualCode)
    {
        if (virtualCode >= 0 && virtualCode < KeyMap.Length && KeyMap[virtualCode] != null)
            return KeyMap[virtualCode]!;

        // cache
        if (UnknownKeys.TryGetValue(virtualCode, out var key)) return key;
        key = new Key(virtualCode, $"Unknown_{virtualCode:X}");
        UnknownKeys[virtualCode] = key;
        return key;
    }
    public static bool operator ==(Key? a, Key? b) => a?.VirtualCode == b?.VirtualCode;
    public static bool operator !=(Key? a, Key? b) => !(a == b);
    public override bool Equals(object? obj) => obj is Key other && VirtualCode == other.VirtualCode;
    public override int GetHashCode() => VirtualCode;
    public override string ToString() => Name;
}
