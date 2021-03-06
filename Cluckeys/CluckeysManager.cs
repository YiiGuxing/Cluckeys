﻿// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using SFML.Audio;

namespace Cluckeys
{
    internal class CluckeysManager : IDisposable
    {
        private const int VK_BACKSPACE = 8; // Backspace
        private const int VK_DELETE = 46; // Delete

        private const float DEFAULT_VOLUME = 70f;

        private bool _initialized;

        private int _keyPressedCount;

        private Sound? _holdSound;
        private readonly SoundPool _soundPool = new SoundPool();

        private SoundBuffer? _defaultSound;
        private readonly Dictionary<int, SoundBuffer> _sounds = new Dictionary<int, SoundBuffer>();
        private readonly Dictionary<int, SoundBuffer> _soundsIgnoreControlKey = new Dictionary<int, SoundBuffer>();

        public bool IsRunning { get; private set; }

        internal static CluckeysManager Instance { get; }

        static CluckeysManager()
        {
            Instance = new CluckeysManager();
        }

        private CluckeysManager()
        {
        }

        private void InitializeSounds()
        {
            if (_initialized) return;

            _defaultSound = new SoundBuffer(Resources.type);
            _holdSound = new Sound(new SoundBuffer(Resources.hold))
            {
                Loop = true,
                Volume = Math.Min(100f, DEFAULT_VOLUME * 1.5f)
            };

            var shiftSound = new SoundBuffer(Resources.shift);
            _soundsIgnoreControlKey[KeyboardHook.VK_SHIFT_L] = shiftSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_SHIFT_R] = shiftSound;

            var controlSound = new SoundBuffer(Resources.control);
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

            var enterSound = new SoundBuffer(Resources.enter);
            // Tab    
            _soundsIgnoreControlKey[9] = enterSound;
            // Enter    
            _soundsIgnoreControlKey[13] = enterSound;

            // ESC
            _soundsIgnoreControlKey[27] = new SoundBuffer(Resources.esc);

            // Windows
            var windowsSound = new SoundBuffer(Resources.windows);
            _soundsIgnoreControlKey[KeyboardHook.VK_WINDOWS_L] = windowsSound;
            _soundsIgnoreControlKey[KeyboardHook.VK_WINDOWS_R] = windowsSound;

            var deleteSound = new SoundBuffer(Resources.delete);
            _soundsIgnoreControlKey[VK_BACKSPACE] = deleteSound;
            _soundsIgnoreControlKey[VK_DELETE] = deleteSound;

            var forwardSound = new SoundBuffer(Resources.forward);
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
            _soundsIgnoreControlKey[37] = new SoundBuffer(Resources.left);
            // Up
            _soundsIgnoreControlKey[38] = new SoundBuffer(Resources.up);
            // Right
            _soundsIgnoreControlKey[39] = new SoundBuffer(Resources.right);
            // Down
            _soundsIgnoreControlKey[40] = new SoundBuffer(Resources.down);

            var symbolSound = new SoundBuffer(Resources.symbol);
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

            _sounds[KeyboardHook.LOCK_SHORTCUT] = new SoundBuffer(Resources.locked);

            var copySound = new SoundBuffer(Resources.copy);
            // Ctrl C
            _sounds[KeyboardHook.CTRL_MASK | 67] = copySound;
            // Ctrl Shift C
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 67] = copySound;

            var pasteSound = new SoundBuffer(Resources.paste);
            // Ctrl V
            _sounds[KeyboardHook.CTRL_MASK | 86] = pasteSound;
            // Ctrl Shift V
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 86] = pasteSound;

            var repentSound = new SoundBuffer(Resources.repent);
            // Ctrl Z
            _sounds[KeyboardHook.CTRL_MASK | 90] = repentSound;
            // Ctrl Shift Z
            _sounds[KeyboardHook.CTRL_MASK | KeyboardHook.SHIFT_MASK | 90] = repentSound;

            // Brackets
            var bracketsSound = new SoundBuffer(Resources.brackets);
            var bracketsSound2 = new SoundBuffer(Resources.brackets2);
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
            if (!IsRunning)
                return;

            _keyPressedCount++;

            var soundBuffer = _soundsIgnoreControlKey.GetValueOrDefault(e.vkCode) ??
                              _sounds.GetValueOrDefault(e.code) ??
                              _defaultSound;
            if (soundBuffer == null)
                return;

            var sound = _soundPool.GetSound();
            sound.SoundBuffer = soundBuffer;
            sound.Play();
        }

        private void OnKeyTypeEvent(KeyboardHook.KeyboardEvent e)
        {
            if (!IsRunning || _keyPressedCount <= 0)
                return;

            var vkCode = e.vkCode;
            if (vkCode == VK_BACKSPACE || vkCode == VK_DELETE)
            {
                _holdSound?.Stop();

                var sound = _soundPool.GetSound();
                sound.SoundBuffer = _soundsIgnoreControlKey[vkCode];
                sound.Play();
            }
            else if (_holdSound != null && _holdSound.Status != SoundStatus.Playing)
            {
                _holdSound.Play();
            }
        }

        private void OnKeyUpEvent(KeyboardHook.KeyboardEvent e)
        {
            if (!IsRunning)
                return;

            _keyPressedCount--;
            if (_keyPressedCount > 0)
                return;

            _keyPressedCount = 0;
            _holdSound?.Stop();
        }

        public void Start()
        {
            if (IsRunning)
                return;

            KeyboardHook.Instance.KeyDown += OnKeyDownEvent;
            KeyboardHook.Instance.KeyType += OnKeyTypeEvent;
            KeyboardHook.Instance.KeyUp += OnKeyUpEvent;

            InitializeSounds();
            IsRunning = true;
            _keyPressedCount = 0;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;


            _holdSound?.Stop();
            KeyboardHook.Instance.KeyDown -= OnKeyDownEvent;
            KeyboardHook.Instance.KeyType -= OnKeyTypeEvent;
            KeyboardHook.Instance.KeyUp -= OnKeyUpEvent;

            _keyPressedCount = 0;
            IsRunning = false;
        }

        public void Dispose()
        {
            Stop();
            _soundPool.Dispose();
            _holdSound?.Dispose();
            _defaultSound?.Dispose();

            foreach (var keyValuePair in _sounds)
            {
                keyValuePair.Value.Dispose();
            }

            foreach (var keyValuePair in _soundsIgnoreControlKey)
            {
                keyValuePair.Value.Dispose();
            }

            _sounds.Clear();
            _soundsIgnoreControlKey.Clear();
            _holdSound = null;
            _initialized = false;
            GC.SuppressFinalize(this);
        }

        private class PooledSound : Sound
        {
            internal long popTime;
        }

        private class SoundPool : IDisposable
        {
            private readonly int _maxPoolSize;
            private readonly List<PooledSound> _pools;

            internal SoundPool(int maxPoolSize = 5)
            {
                if (maxPoolSize <= 0)
                {
                    throw new ArgumentException("maxPoolSize must be greater than 0.");
                }

                _maxPoolSize = maxPoolSize;
                _pools = new List<PooledSound>(Math.Min(maxPoolSize, 5));
            }

            private PooledSound? FindPooledSound()
            {
                PooledSound? sound = null;
                foreach (var pooledSound in _pools)
                {
                    if (pooledSound.Status == SoundStatus.Stopped)
                    {
                        return pooledSound;
                    }

                    if (sound == null || pooledSound.popTime < sound.popTime)
                    {
                        sound = pooledSound;
                    }
                }

                if (_pools.Count < _maxPoolSize)
                    return null;

                sound?.Stop();
                return sound;
            }

            internal Sound GetSound()
            {
                var sound = FindPooledSound();
                if (sound == null)
                {
                    _pools.Add(sound = new PooledSound
                    {
                        Volume = DEFAULT_VOLUME
                    });
                }

                sound.popTime = Environment.TickCount64;
                return sound;
            }

            public void Dispose()
            {
                foreach (var pooledSound in _pools)
                {
                    pooledSound.Stop();
                    pooledSound.Dispose();
                }

                _pools.Clear();
            }
        }
    }
}