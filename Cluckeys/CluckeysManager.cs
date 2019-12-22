// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using SFML.Audio;

namespace Cluckeys
{
    internal class CluckeysManager
    {
        private const int VK_BACKSPACE = 8; // Backspace
        private const int VK_DELETE = 46; // Delete
        private const int VK_SHIFT_L = 160; // Left Shift
        private const int VK_SHIFT_R = 161; // Right Shift
        private const int VK_CTRL_L = 162; // Left Ctrl
        private const int VK_CTRL_R = 163; // Right Ctrl
        private const int VK_ALT_L = 164; // Left Alt
        private const int VK_ALT_R = 165; // Right Alt

        private const int SHIFT_MASK = 1 << 8;
        private const int CTRL_MASK = 1 << 9;
        private const int ALT_MASK = 1 << 10;

        private bool _initialized;
        
        private readonly KeyboardHook _keyboardHook;

        private int _modifiers;
        private int _keyPressedCount;

        private Sound _sound;
        private Sound _holdSound;

        private SoundBuffer _defaultSound;
        private readonly Dictionary<int, SoundBuffer> _sounds = new Dictionary<int, SoundBuffer>();
        private readonly Dictionary<int, SoundBuffer> _soundsIgnoreControlKey = new Dictionary<int, SoundBuffer>();

        public static CluckeysManager Instance { get; }

        static CluckeysManager()
        {
            Instance = new CluckeysManager();
        }

        private CluckeysManager()
        {
            _keyboardHook = new KeyboardHook
            {
                OnKeyDownEvent = OnKeyDownEvent,
                OnKeyTypeEvent = OnKeyTypeEvent,
                OnKeyUpEvent = OnKeyUpEvent
            };
        }

        private void InitializeSounds()
        {
            if (_initialized) return;

            _defaultSound = new SoundBuffer("sounds\\type.flac");
            _sound = new Sound(_defaultSound);
            _holdSound = new Sound(new SoundBuffer("sounds\\hold.flac")) {Loop = true};

            var shiftSound = new SoundBuffer("sounds\\shift.flac");
            _soundsIgnoreControlKey[VK_SHIFT_L] = shiftSound;
            _soundsIgnoreControlKey[VK_SHIFT_R] = shiftSound;

            var controlSound = new SoundBuffer("sounds\\control.flac");
            _soundsIgnoreControlKey[VK_CTRL_L] = controlSound;
            _soundsIgnoreControlKey[VK_CTRL_R] = controlSound;
            _soundsIgnoreControlKey[VK_ALT_L] = controlSound;
            _soundsIgnoreControlKey[VK_ALT_R] = controlSound;

            // Caps
            _soundsIgnoreControlKey[20] = controlSound;
            // Insert
            _soundsIgnoreControlKey[45] = controlSound;
            // Menu
            _soundsIgnoreControlKey[93] = controlSound;
            // Num
            _soundsIgnoreControlKey[144] = controlSound;

            var enterSound = new SoundBuffer("sounds\\enter.flac");
            // Tab    
            _soundsIgnoreControlKey[9] = enterSound;
            // Enter    
            _soundsIgnoreControlKey[13] = enterSound;

            // ESC
            _soundsIgnoreControlKey[27] = new SoundBuffer("sounds\\esc.flac");
            // windows
            _soundsIgnoreControlKey[91] = new SoundBuffer("sounds\\windows.flac");

            var deleteSound = new SoundBuffer("sounds\\delete.flac");
            _soundsIgnoreControlKey[VK_BACKSPACE] = deleteSound;
            _soundsIgnoreControlKey[VK_DELETE] = deleteSound;

            var forwardSound = new SoundBuffer("sounds\\forward.flac");
            // PageUp
            _soundsIgnoreControlKey[33] = forwardSound;
            // PageDown
            _soundsIgnoreControlKey[34] = forwardSound;
            // End
            _soundsIgnoreControlKey[35] = forwardSound;
            // Home
            _soundsIgnoreControlKey[36] = forwardSound;

            // Arrows
            // Left
            _soundsIgnoreControlKey[37] = new SoundBuffer("sounds\\left.flac");
            // Up
            _soundsIgnoreControlKey[38] = new SoundBuffer("sounds\\up.flac");
            // Right
            _soundsIgnoreControlKey[39] = new SoundBuffer("sounds\\right.flac");
            // Down
            _soundsIgnoreControlKey[40] = new SoundBuffer("sounds\\down.flac");

            var symbolSound = new SoundBuffer("sounds\\symbol.flac");
            // Space
            _soundsIgnoreControlKey[32] = symbolSound;
            // `~
            _soundsIgnoreControlKey[192] = symbolSound;
            // -_
            _soundsIgnoreControlKey[189] = symbolSound;
            // =+
            _soundsIgnoreControlKey[187] = symbolSound;
            // \|
            _soundsIgnoreControlKey[220] = symbolSound;
            // ;:
            _soundsIgnoreControlKey[186] = symbolSound;
            // '"
            _soundsIgnoreControlKey[222] = symbolSound;
            // ,<
            _soundsIgnoreControlKey[188] = symbolSound;
            // .>
            _soundsIgnoreControlKey[190] = symbolSound;
            // /?
            _soundsIgnoreControlKey[191] = symbolSound;
            // Num(/)
            _soundsIgnoreControlKey[111] = symbolSound;
            // Num(*)
            _soundsIgnoreControlKey[106] = symbolSound;
            // Num(-)
            _soundsIgnoreControlKey[109] = symbolSound;
            // Num(+)
            _soundsIgnoreControlKey[107] = symbolSound;
            // Num(.)
            _soundsIgnoreControlKey[110] = symbolSound;

            // Key Combinations
            _sounds[SHIFT_MASK | 49] = symbolSound;
            _sounds[SHIFT_MASK | 50] = symbolSound;
            _sounds[SHIFT_MASK | 51] = symbolSound;
            _sounds[SHIFT_MASK | 52] = symbolSound;
            _sounds[SHIFT_MASK | 53] = symbolSound;
            _sounds[SHIFT_MASK | 54] = symbolSound;
            _sounds[SHIFT_MASK | 55] = symbolSound;
            _sounds[SHIFT_MASK | 56] = symbolSound;

            var copySound = new SoundBuffer("sounds\\copy.flac");
            // Ctrl C
            _sounds[CTRL_MASK | 67] = copySound;
            // Ctrl Shift C
            _sounds[CTRL_MASK | SHIFT_MASK | 67] = copySound;

            var pasteSound = new SoundBuffer("sounds\\paste.flac");
            // Ctrl V
            _sounds[CTRL_MASK | 86] = pasteSound;
            // Ctrl Shift V
            _sounds[CTRL_MASK | SHIFT_MASK | 86] = pasteSound;

            var repentSound = new SoundBuffer("sounds\\repent.flac");
            // Ctrl Z
            _sounds[CTRL_MASK | 90] = repentSound;
            // Ctrl Shift Z
            _sounds[CTRL_MASK | SHIFT_MASK | 90] = repentSound;

            // Brackets
            var bracketsSound = new SoundBuffer("sounds\\brackets.flac");
            var bracketsSound2 = new SoundBuffer("sounds\\brackets2.flac");
            // [{
            _soundsIgnoreControlKey[219] = bracketsSound;
            // ]}
            _soundsIgnoreControlKey[221] = bracketsSound;
            // (
            _sounds[SHIFT_MASK | 57] = bracketsSound2;
            // )
            _sounds[SHIFT_MASK | 48] = bracketsSound2;

            _initialized = true;
        }


        private void OnKeyDownEvent(KeyboardHook.KeyboardEvent e)
        {
            _keyPressedCount++;

            var vkCode = e.vkCode;
            AddModifierIfNeed(vkCode);

            var soundBuffer = _soundsIgnoreControlKey.GetValueOrDefault(vkCode) ??
                              _sounds.GetValueOrDefault(_modifiers | vkCode, _defaultSound);
            if (soundBuffer == null) return;

            _sound.Stop();
            _sound.SoundBuffer = soundBuffer;
            _sound.Play();
        }

        private void AddModifierIfNeed(int vkCode)
        {
            switch (vkCode)
            {
                case VK_SHIFT_L:
                case VK_SHIFT_R:
                    _modifiers |= SHIFT_MASK;
                    break;
                case VK_CTRL_L:
                case VK_CTRL_R:
                    _modifiers |= CTRL_MASK;
                    break;
                case VK_ALT_L:
                case VK_ALT_R:
                    _modifiers |= ALT_MASK;
                    break;
            }
        }

        private void OnKeyTypeEvent(KeyboardHook.KeyboardEvent e)
        {
            var vkCode = e.vkCode;
            if (vkCode == VK_BACKSPACE || vkCode == VK_DELETE)
            {
                _holdSound.Stop();
                _sound.Stop();
                _sound.SoundBuffer = _soundsIgnoreControlKey[vkCode];
                _sound.Play();
            }
            else if (_holdSound.Status != SoundStatus.Playing)
            {
                _holdSound.Play();
            }
        }

        private void OnKeyUpEvent(KeyboardHook.KeyboardEvent e)
        {
            _keyPressedCount--;
            RemoveModifierIfNeed(e.vkCode);
            if (_keyPressedCount == 0)
            {
                _holdSound.Stop();
            }
        }

        private void RemoveModifierIfNeed(int vkCode)
        {
            switch (vkCode)
            {
                case VK_SHIFT_L:
                case VK_SHIFT_R:
                    _modifiers &= ~SHIFT_MASK;
                    break;
                case VK_CTRL_L:
                case VK_CTRL_R:
                    _modifiers &= ~CTRL_MASK;
                    break;
                case VK_ALT_L:
                case VK_ALT_R:
                    _modifiers &= ~ALT_MASK;
                    break;
            }
        }

        public void Start()
        {
            InitializeSounds();
            _keyboardHook.Start();
        }

        public void Stop()
        {
            _keyboardHook.Stop();
        }

        ~CluckeysManager()
        {
            Stop();
        }
    }
}