using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace GDIVirus.Audio
{
    public class PCM_Audio
    {
        public enum Waves
        {
            Sine = 0,
            Square = 1,
            Sawtooth = 2,
            Triangle = 3,
            Whitenoise = 4
        }

        public void PCM(Waves waveType, int sampleRate, int duration, float[] freqs, int tempo)
        {
            byte[] chunkID = Encoding.ASCII.GetBytes("RIFF");
            short numChannels = 2;
            int numSamples = sampleRate * duration;
            short bitsPerSample = 16;
            int subchunk2Size = numSamples * numChannels * bitsPerSample / 8;
            int chunkSize = 36 + subchunk2Size;
            byte[] format = Encoding.ASCII.GetBytes("WAVE");
            byte[] subchunk1ID = Encoding.ASCII.GetBytes("fmt ");
            int subchunk1Size = 16;
            short audioFormat = 1;
            int byteRate = sampleRate * numChannels * bitsPerSample / 8;
            short blockAlign = (short)(numChannels * bitsPerSample / 8);
            byte[] subchunk2ID = Encoding.ASCII.GetBytes("data");

            int amplitude = short.MaxValue / 2;

            short[] wave = new short[numSamples * numChannels];
            byte[] wave_byte = new byte[numSamples * numChannels * sizeof(short)];

            short[] new_wave = new_waves(waveType, wave, numSamples, sampleRate, numChannels, amplitude, freqs, tempo);

            Buffer.BlockCopy(wave, 0, wave_byte, 0, wave.Length * sizeof(short));
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(mem))
                {
                    wr.Write(chunkID);
                    wr.Write(chunkSize);
                    wr.Write(format);
                    wr.Write(subchunk1ID);
                    wr.Write(subchunk1Size);
                    wr.Write(audioFormat);
                    wr.Write(numChannels);
                    wr.Write(sampleRate);
                    wr.Write(byteRate);
                    wr.Write(blockAlign);
                    wr.Write(bitsPerSample);
                    wr.Write(subchunk2ID);
                    wr.Write(subchunk2Size);

                    wr.Write(wave_byte);
                    SoundPlayer sound = new SoundPlayer(mem);

                    mem.Position = 0;
                    sound.PlayLooping();
                    sound.Dispose();
                }
            }
        }

        public short[] new_waves(Waves waves, short[] old_wave, int numSamples, int sampleRate, int numChannels, int amplitude, float[] frequency, int tempo)
        {
            short[] wave = old_wave;
            int freq_row = -1;
            int tempo_count = 0;

            double timeIncrement = 1.0 / sampleRate;

            Random random;

            switch (waves)
            {
                case Waves.Sine:
                    for (int i = 0; i < numSamples * numChannels; i++)
                    {
                        if (tempo_count == 0)
                        {
                            if (freq_row < frequency.Length - 1)
                                freq_row++;
                            else
                                freq_row = 0;
                        }

                        tempo_count++;

                        if (tempo_count == (numSamples * numChannels) / tempo)
                            tempo_count = 0;

                        float freq = frequency[freq_row];
                        for (int channel = 0; channel < numChannels; channel++)
                        {
                            wave[i] = Convert.ToInt16(amplitude * Math.Sin(freq * i));
                        }
                    }

                    break;
                case Waves.Square:
                    for (int i = 0; i < numSamples * numChannels; i++)
                    {
                        if (tempo_count == 0)
                        {
                            if (freq_row < frequency.Length - 1)
                                freq_row++;
                            else
                                freq_row = 0;
                        }

                        tempo_count++;

                        if (tempo_count == (numSamples * numChannels) / tempo)
                            tempo_count = 0;

                        float freq = frequency[freq_row];
                        for (int channel = 0; channel < numChannels; channel++)
                        {
                            wave[i] = Convert.ToInt16(amplitude * Math.Sign(Math.Sin(freq * i)));
                        }
                    }

                    break;
                case Waves.Sawtooth:
                    for (int i = 0; i < numSamples * numChannels; i++)
                    {
                        if (tempo_count == 0)
                        {
                            if (freq_row < frequency.Length - 1)
                                freq_row++;
                            else
                                freq_row = 0;
                        }

                        tempo_count++;

                        if (tempo_count == (numSamples * numChannels) / tempo)
                            tempo_count = 0;

                        float freq = frequency[freq_row];

                        double t = i * timeIncrement;
                        for (int channel = 0; channel < numChannels; channel++)
                        {
                            wave[i] = Convert.ToInt16(amplitude * (2.0 * (t * freq - Math.Floor(0.5 + t * freq))));
                        }
                    }

                    break;
                case Waves.Triangle:
                    for (int i = 0; i < numSamples * numChannels; i++)
                    {
                        if (tempo_count == 0)
                        {
                            if (freq_row < frequency.Length - 1)
                                freq_row++;
                            else
                                freq_row = 0;
                        }

                        tempo_count++;

                        if (tempo_count == (numSamples * numChannels) / tempo)
                            tempo_count = 0;

                        float freq = frequency[freq_row];

                        double t = i * timeIncrement;
                        for (int channel = 0; channel < numChannels; channel++)
                        {
                            wave[i] = Convert.ToInt16(amplitude * (2.0 * Math.Abs(2.0 * (t * freq - Math.Floor(t * freq + 0.5))) - 1.0));
                        }
                    }

                    break;
                case Waves.Whitenoise:
                    random = new Random();

                    for (int i = 0; i < numSamples * numChannels; i++)
                    {
                        wave[i] = Convert.ToInt16(random.Next(-amplitude, amplitude));
                    }

                    break;
            }

            return wave;
        }
    }
}
