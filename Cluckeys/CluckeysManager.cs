// ReSharper disable InconsistentNaming

using System.Collections.Generic;
using SFML.Audio;

namespace Cluckeys
{
    internal class CluckeysManager
    {
        private const int VK_BACKSPACE = 8; // Backspace
        private const int VK_DELETE = 46; // Delete

        private bool _initialized;

        private readonly KeyboardHook _keyboardHook;

        private int _keyPressedCount;

        private Sound? _sound;
        private Sound? _holdSound;

        private SoundBuffer? _defaultSound;
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
            _soundsIgnoreControlKey[KeyboardHook.VK_SHIFT_L] = shiftSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_SHIFT_R] = shiftSound;

            var controlSound = new SoundBuffer("sounds\\control.flac");
            _soundsIgnoreControlKey[KeyboardHook.VK_CTRL_L] = controlSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_CTRL_R] = controlSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_ALT_L] = controlSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_ALT_R] = controlSound;

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
            _soundsIgnoreControlKey[KeyboardHook.VK_WINDOWS] = new SoundBuffer("sounds\\windows.flac");

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
            _sounds[KeyboardHook.SHIFT_MASK | 49] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 50] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 51] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 52] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 53] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 54] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 55] = symbolSound;
            _sounds[KeyboardHook.SHIFT_MASK | 56] = symbolSound;

            _sounds[KeyboardHook.LOCK_SHORTCUT] = new SoundBuffer("sounds\\locked.flac");

            var copySound = new SoundBuffer("sounds\\copy.flac");
            // Ctrl C
            _sounds[KeyboardHook.CTRL_MASK | 67] = copySound;
            // Ctrl Shift C
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 67] = copySound;

            var pasteSound = new SoundBuffer("sounds\\paste.flac");
            // Ctrl V
            _sounds[KeyboardHook.CTRL_MASK | 86] = pasteSound;
            // Ctrl Shift V
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 86] = pasteSound;

            var repentSound = new SoundBuffer("sounds\\repent.flac");
            // Ctrl Z
            _sounds[KeyboardHook.CTRL_MASK | 90] = repentSound;
            // Ctrl Shift Z
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 90] = repentSound;

            // Brackets
            var bracketsSound = new SoundBuffer("sounds\\brackets.flac");
            var bracketsSound2 = new SoundBuffer("sounds\\brackets2.flac");
            // [{
            _soundsIgnoreControlKey[219] = bracketsSound;
            // ]}
            _soundsIgnoreControlKey[221] = bracketsSound;
            // (
            _sounds[KeyboardHook.SHIFT_MASK | 57] = bracketsSound2;
            // )
            _sounds[KeyboardHook.SHIFT_MASK | 48] = bracketsSound2;

            _initialized = true;
        }

        private void OnKeyDownEvent(KeyboardHook.KeyboardEvent e)
        {
            _keyPressedCount++;

            if (_sound == null)
                return;

            var soundBuffer = _soundsIgnoreControlKey.GetValueOrDefault(e.vkCode) ??
                              _sounds.GetValueOrDefault(e.code) ??
                              _defaultSound;
            if (soundBuffer == null)
                return;

            _sound.Stop();
            _sound.SoundBuffer = soundBuffer;
            _sound.Play();
        }

        private void OnKeyTypeEvent(KeyboardHook.KeyboardEvent e)
        {
            var vkCode = e.vkCode;
            if (vkCode == VK_BACKSPACE || vkCode == VK_DELETE)
            {
                _holdSound?.Stop();
                if (_sound != null)
                {
                    _sound.Stop();
                    _sound.SoundBuffer = _soundsIgnoreControlKey[vkCode];
                    _sound.Play();
                }
            }
            else if (_holdSound != null && _holdSound.Status != SoundStatus.Playing)
            {
                _holdSound.Play();
            }
        }

        private void OnKeyUpEvent(KeyboardHook.KeyboardEvent e)
        {
            _keyPressedCount--;
            if (_keyPressedCount > 0)
                return;

            _keyPressedCount = 0;
            _holdSound?.Stop();
        }

        public void Start()
        {
            InitializeSounds();
            _keyPressedCount = 0;
            _keyboardHook.Start();
        }

        public void Stop()
        {
            _keyboardHook.Stop();
            _holdSound?.Stop();
            _keyPressedCount = 0;
        }

        ~CluckeysManager()
        {
            Stop();
        }
    }
}