import numpy as np
import soundfile as sf
from scipy.signal import butter, lfilter
import random

print("🔥 Creating ULTIMATE Firelink Shrine theme...")

duration = 120.0
sample_rate = 44100
key_freq = 146.83  # D3
random.seed(42)

t = np.linspace(0, duration, int(sample_rate * duration), False)

# --- CORE FIRELINK SHRINE ELEMENTS ---
def create_signature_harp_melody():
    """The iconic Firelink Shrine harp melody pattern"""
    harp = np.zeros_like(t)
    
    # Classic Firelink melody pattern (simplified)
    # Based on the actual game's melody structure
    melody_pattern = [
        (key_freq, 2.0),           # D
        (key_freq * 1.2, 1.5),     # F  
        (key_freq * 1.5, 2.5),     # A
        (key_freq * 2, 1.0),       # D (octave)
        (key_freq * 1.5, 1.5),     # A
        (key_freq * 1.2, 2.0),     # F
        (key_freq, 3.0)            # D (resolve)
    ]
    
    # Play this pattern multiple times throughout - CONSISTENT VOLUME
    pattern_starts = [8, 35, 62, 89]
    
    for i, pattern_start in enumerate(pattern_starts):
        current_time = pattern_start
        
        # FIXED VOLUME - no progressive increase
        pattern_volume = 0.2  # Same volume for all patterns
        
        for freq, note_duration in melody_pattern:
            if current_time + note_duration < duration:
                start_idx = int(current_time * sample_rate)
                note_len = int(note_duration * sample_rate)
                end_idx = min(start_idx + note_len, len(harp))
                
                if end_idx > start_idx:
                    note_t = np.linspace(0, note_duration, end_idx - start_idx)
                    
                    # Simple clean decay without complex envelope
                    envelope = np.exp(-note_t * 1.0)
                    
                    # Simpler harp harmonics
                    fundamental = np.sin(2 * np.pi * freq * note_t)
                    second = 0.3 * np.sin(2 * np.pi * freq * 2 * note_t)
                    
                    harp_note = (fundamental + second) * envelope
                    harp[start_idx:end_idx] += harp_note * pattern_volume
            
            current_time += note_duration
    
    return harp

def create_warm_string_pad():
    """Warm, enveloping string pad like Firelink's background"""
    strings = np.zeros_like(t)
    
    # D minor tonality with suspended chords
    string_voices = [
        key_freq * 0.67,    # Bb2  
        key_freq,           # D3
        key_freq * 1.2,     # F3
        key_freq * 1.5,     # A3
        key_freq * 2,       # D4
    ]
    
    for freq in string_voices:
        # Much slower and smaller vibrato
        vibrato = 1 + 0.0002 * np.sin(2 * np.pi * 0.8 * t + freq/100)
        
        # String voice with natural bow texture
        voice = np.sin(2 * np.pi * freq * vibrato * t)
        voice += 0.3 * np.sin(2 * np.pi * freq * 2 * vibrato * t)  # Second harmonic
        
        strings += voice
    
    # Steady level instead of swells to avoid pulsing
    steady_envelope = np.ones_like(t) * 0.5
    
    return strings * steady_envelope * 0.05

def create_gentle_bells():
    """Soft, distant bells like Firelink's atmosphere"""
    bells = np.zeros_like(t)
    
    # Bell frequencies in Firelink's range
    bell_freqs = [key_freq * 2, key_freq * 3, key_freq * 4, key_freq * 5]
    
    # Sparse bell strikes throughout - CONSISTENT TIMING AND VOLUME
    bell_times = [20, 38, 58, 76, 98]  # Removed late bell to avoid buildup
    
    for i, bell_time in enumerate(bell_times):
        if random.random() < 0.6:  # Reduced chance, more sparse
            freq = random.choice(bell_freqs)
            
            start_idx = int(bell_time * sample_rate)
            bell_duration = 5.0  # Shorter duration
            bell_len = int(bell_duration * sample_rate)
            end_idx = min(start_idx + bell_len, len(bells))
            
            if end_idx > start_idx:
                bell_t = np.linspace(0, bell_duration, end_idx - start_idx)
                
                # Simple clean envelope
                envelope = np.exp(-bell_t * 0.8)
                
                # Simpler bell tone
                bell_tone = np.sin(2 * np.pi * freq * bell_t) * envelope
                
                # CONSISTENT VOLUME - no increase over time
                bells[start_idx:end_idx] += bell_tone * 0.06
    
    return bells

def create_firelink_ambience():
    """The underlying ambience that makes Firelink feel alive"""
    # Gentle harmonic drone - NO breathing pattern
    drone1 = np.sin(2 * np.pi * key_freq * t)
    drone2 = np.sin(2 * np.pi * key_freq * 1.5 * t)  # Perfect fifth
    drone3 = np.sin(2 * np.pi * key_freq * 2 * t)    # Octave
    
    # Steady level instead of breathing pattern
    steady_level = 0.8  # Constant level, no pulsing
    
    # Combine drones
    drone = (drone1 + 0.6 * drone2 + 0.4 * drone3) * steady_level
    
    # Add subtle texture
    texture_noise = np.random.randn(len(t)) * 0.003
    b, a = butter(2, [80, 1200], btype='band', fs=sample_rate)
    texture = lfilter(b, a, texture_noise)
    
    return (drone * 0.08) + texture

def create_subtle_flute():
    """Occasional flute-like melody fragments - NO breath envelope"""
    flute = np.zeros_like(t)
    
    # Gentle melody fragments at sparse intervals
    fragment_times = [45, 85]  # Only two gentle appearances
    
    fragment_notes = [
        (key_freq * 2, 2.5),      # D4
        (key_freq * 2.25, 1.5),   # F#4
        (key_freq * 2.67, 2.0),   # A4
        (key_freq * 3, 3.0)       # D5
    ]
    
    for fragment_start in fragment_times:
        current_time = fragment_start
        
        for freq, note_duration in fragment_notes:
            if current_time + note_duration < duration:
                start_idx = int(current_time * sample_rate)
                note_len = int(note_duration * sample_rate)
                end_idx = min(start_idx + note_len, len(flute))
                
                if end_idx > start_idx:
                    note_t = np.linspace(0, note_duration, end_idx - start_idx)
                    
                    # Simple smooth envelope without breathing pattern
                    envelope = np.sin(np.linspace(0, np.pi, len(note_t))) ** 0.5
                    
                    # Steady flute timbre without vibrato
                    flute_tone = (np.sin(2 * np.pi * freq * note_t) +
                                0.2 * np.sin(2 * np.pi * freq * 2 * note_t)) * envelope
                    
                    flute[start_idx:end_idx] += flute_tone * 0.06
            
            current_time += note_duration + 0.5  # Small gaps between notes
    
    return flute

print("✓ Creating authentic Firelink Shrine elements...")

# Create all elements with Firelink character
harp_melody = create_signature_harp_melody()
string_pad = create_warm_string_pad()
bells = create_gentle_bells()
ambience = create_firelink_ambience()
flute = create_subtle_flute()

print("✓ All elements created")

# --- Reduced reverb to prevent buildup ---
def firelink_reverb(signal, delay_sec=0.08, decay=0.25, wet=0.15):
    delay_samples = int(delay_sec * sample_rate)
    reverb = np.zeros_like(signal)
    if delay_samples < len(signal):
        reverb[delay_samples:] = signal[:-delay_samples] * decay
    return signal + reverb * wet

# Apply lighter reverb to prevent accumulation
harp_rev = firelink_reverb(harp_melody, 0.08, 0.3, 0.2)
strings_rev = firelink_reverb(string_pad, 0.1, 0.25, 0.15)
bells_rev = firelink_reverb(bells, 0.12, 0.35, 0.2)
flute_rev = firelink_reverb(flute, 0.06, 0.2, 0.15)
ambience_rev = firelink_reverb(ambience, 0.04, 0.2, 0.1)

# --- Consistent Mix - no buildup over time ---
mix = (ambience_rev * 0.8 +      # Reduced foundation ambience
       harp_rev * 1.2 +          # Slightly reduced harp melody
       strings_rev * 0.8 +       # Reduced string support
       bells_rev * 0.7 +         # Reduced bell accents
       flute_rev * 0.6)          # Reduced melodic fragments

# --- Warm, intimate filtering ---
def firelink_eq(data):
    # Light highpass to clean up mud
    b, a = butter(1, 35 / (0.5 * sample_rate), btype='high')
    data = lfilter(b, a, data)
    
    # Gentle presence boost
    b, a = butter(1, [800, 3000], btype='band', fs=sample_rate)
    presence = lfilter(b, a, data) * 0.1
    data = data + presence
    
    # Warm lowpass
    b, a = butter(2, 4500 / (0.5 * sample_rate), btype='low')
    data = lfilter(b, a, data)
    
    return data

mix = firelink_eq(mix)

# Final processing for Firelink character
mix = mix - np.mean(mix)  # Remove DC

# Normalize to comfortable listening level
peak = np.max(np.abs(mix))
if peak > 0:
    mix = mix / peak * 0.8  # Good level for ambience

# Gentle limiting
mix = np.tanh(mix * 1.1) * 0.9
mix = np.clip(mix, -1.0, 1.0)

final_rms = np.sqrt(np.mean(mix**2))
print(f"✓ Final RMS: {final_rms:.4f}")
print(f"✓ Final Peak: {np.max(np.abs(mix)):.4f}")

# Export
output_file = "dark_souls_FIRELINK_ULTIMATE.wav"
sf.write(output_file, mix, sample_rate)

print(f"✅ Export complete: {output_file}")
print(f"\n🔥⚔️ ULTIMATE FIRELINK SHRINE VERSION:")
print(f"• Authentic harp melody pattern from the game")
print(f"• Warm string pad in D minor tonality")
print(f"• Gentle bells with natural spacing")
print(f"• Subtle flute melody fragments")
print(f"• Living ambience with harmonic drones")
print(f"• Intimate reverb (not cathedral-scale)")
print(f"• Warm EQ for cozy atmosphere")
print(f"• Perfect balance of melancholy and comfort")
print(f"• This captures the TRUE essence of Firelink Shrine!")
print(f"• RMS: {final_rms:.3f} - perfect for contemplative listening")