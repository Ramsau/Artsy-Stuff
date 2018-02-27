using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Audioframe
{
    public partial class Form1 : Form
    {
        Bitmap image, tempImage;
        NAudio.Wave.WaveFileReader wave;
        int flatten = 1, sampleSize = 650, passedSamples = 0, passedChunks=0;
        float[] vol = { 0, 0 }, pitch = { 0, 0 };
        double currentSampleRate = 0;
        List<string> readPlaylist = new List<string>();
        string appDataFolder;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\Audioframe\\";
            timerScroll.Start();
        }

        private void timerScroll_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.playState != WMPLib.WMPPlayState.wmppsPlaying)
                return;
            
            if (currentSampleRate == 0)
            {
                currentSampleRate = getSampleRate();
            }

            double passedFrames = mediaPlayer.Ctlcontrols.currentPosition*currentSampleRate;
            double realPassedSamples = passedFrames / sampleSize;
            pictureBox1.Left = (int)(-realPassedSamples)+passedChunks*image.Width/2;
            if(-pictureBox1.Left >= picPanel.Width)
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    g.DrawImage(tempImage, 0, 0);
                }
                pictureBox1.Left = 0;
                updateImage();
                passedChunks++;
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Audio Files (*.wav, *.mp3)|*.wav; *.mp3";
            open.Multiselect = true;
            open.Title = "Choose songs";
            DialogResult dr = open.ShowDialog();
            if (dr == DialogResult.OK)
            {
                WMPLib.IWMPPlaylist playlist = mediaPlayer.playlistCollection.newPlaylist("Current Playlist");
                foreach(string file in open.FileNames)
                {
                    WMPLib.IWMPMedia media = mediaPlayer.newMedia(file);
                    playlist.appendItem(media);
                    if (file.EndsWith(".mp3"))
                    {
                        using (NAudio.Wave.Mp3FileReader mp3 = new NAudio.Wave.Mp3FileReader(file))
                        {
                            using (NAudio.Wave.WaveStream pcm = NAudio.Wave.WaveFormatConversionStream.CreatePcmStream(mp3))
                            {
                                int startIndex = file.LastIndexOf('\\');
                                string tempFile = appDataFolder + file.Substring(startIndex)+".temp.wav";
                                Directory.CreateDirectory(tempFile.Substring(0,tempFile.LastIndexOf('\\')));
                                NAudio.Wave.WaveFileWriter.CreateWaveFile(tempFile, mp3);
                                readPlaylist.Add(tempFile);
                            }
                        }
                    }
                    else
                    {
                        readPlaylist.Add(file);
                    }
                }
                mediaPlayer.currentPlaylist = playlist;
                mediaPlayer.Ctlcontrols.play();
            }
        }

        public Color Hue(float hue)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60), value=128;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(0);
            int q = Convert.ToInt32(value*(1 - f));
            int t = Convert.ToInt32(value*f);
            if (hi == 0) return Color.FromArgb(255, v, t, p);
            else if (hi == 1) return Color.FromArgb(255, q, v, p);
            else if (hi == 2) return Color.FromArgb(255, p, v, t);
            else if (hi == 3) return Color.FromArgb(255, p, q, v);
            else if (hi == 4) return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void mediaPlayer_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {
            int index = 0;
            for (index=0; index<mediaPlayer.currentPlaylist.count-1; index++)
            {
                if (mediaPlayer.currentMedia.isIdentical[mediaPlayer.currentPlaylist.Item[index]])
                    break;
            }
            wave = new NAudio.Wave.WaveFileReader(readPlaylist[index]);
            passedChunks = 0;
            image = tempImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            updateImage();
            getSampleRate();
            using (Graphics g = Graphics.FromImage(image))
            {
                g.DrawImage(tempImage, 0, 0);
            }
            pictureBox1.Image = image;
        }

        private void mediaPlayer_PositionChange(object sender, AxWMPLib._WMPOCXEvents_PositionChangeEvent e)
        {
            double passedFrames = e.newPosition * currentSampleRate;
            double realPassedSamples = passedFrames / sampleSize;
            passedChunks = (int)realPassedSamples % (image.Width * 2);
            passedSamples = (int)realPassedSamples - (int)(realPassedSamples % image.Width);
            wave.Position = (int)passedSamples * sampleSize;
            updateImage();
            using (Graphics g = Graphics.FromImage(image))
            {
                g.DrawImage(tempImage, 0, 0);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            tempImage.Dispose();
            WMPLib.IWMPPlaylist playlist = mediaPlayer.playlistCollection.newPlaylist("Current Playlist");
            mediaPlayer.currentPlaylist = playlist;
            mediaPlayer.Dispose();
            wave.Dispose();
            foreach (string file in readPlaylist)
            {
                if (file.EndsWith(".temp.wav"))
                {
                    Retry:
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }

        public void nextRender(NAudio.Wave.WaveFileReader wavReader)
        {
            //[0] for right, [1] for left
            float[,] sample = new float[sampleSize,2];

            for (int i = 0; i < sampleSize; i++)
            {
                float[] frame= wavReader.ReadNextSampleFrame();
                for (int x = 0; x < 2; x++) {
                    try
                    {
                        sample[i, x] = frame[x];
                    }
                    catch
                    {
                        vol = null;
                        pitch = null;
                        return;
                    }
                }
            }
            passedSamples++;

            for (int side = 0; side < 2; side++)
            {
                float avgPeak = 0, avgLow = 0;
                float lastFrame = 0;
                int curveCount = 0;
                bool rising = true;
                List<float> peaks = new List<float>(), lows = new List<float>();

                for (int samplePos=0; samplePos<sampleSize;samplePos++)
                {
                    float frame = sample[samplePos, side];
                    if (rising != (0 < frame))
                    {
                        if (rising)
                            lows.Add(frame);
                        else
                            peaks.Add(frame);

                        rising = !rising;
                        curveCount++;
                        lastFrame = frame;
                    }
                }

                if (lows.Count == 0)
                    lows.Add(0);

                if (peaks.Count == 0)
                    peaks.Add(0);

                foreach (float low in lows)
                {
                    avgLow += low;
                }
                avgLow /= curveCount;
                foreach (float peak in peaks)
                {
                    avgPeak += peak;
                }
                avgPeak /= curveCount;
                vol[side] = avgPeak - avgLow;

                pitch[side] = (float)curveCount / (float)sampleSize;
            }
        }

        public Bitmap renderChunk(int width, int height)
        {
            Bitmap drawing = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(drawing))
            {
                g.Clear(Color.White);
            }
            for (int passed = 0; passed < width; passed++)
            {
                nextRender(wave);
                if (vol == null)
                    return drawing;

                for (int side = 0; side < 2; side++)
                {
                    //visualise
                    float drawHeight = (float)drawing.Height / 2, ampVol = vol[side] * 300;
                    Point dest = new Point(passed, 0);
                    if (side == 0)
                    {
                        dest.Y = (int)(drawHeight / ((int)ampVol != 0 ? ampVol : 1));
                    }
                    else
                    {
                        dest.Y = (int)(2 * drawHeight - (drawHeight / ((int)ampVol != 0 ? ampVol : 1)));
                    }
                    using (Graphics g = Graphics.FromImage(drawing))
                    {
                        g.DrawLine(new Pen(Hue(75-(360*pitch[side]*2)), 1), new Point(passed, (int)drawHeight - 1), dest);
                        g.Save();
                    }
                }
            }
            return drawing;
        }

        public void updateImage()
        {
            Graphics g = Graphics.FromImage(tempImage);
            //draw right half of image to the left side
            Bitmap remain = CropImage(image, new Rectangle(tempImage.Width / 2, 0, tempImage.Width / 2, tempImage.Height));
            g.DrawImage(remain, new Point(0, 0));

            //draw new chunk to the right side 
            g.DrawImage(renderChunk(tempImage.Width / 2, tempImage.Height), new Point(tempImage.Width / 2, 0));

        }

        public int getSampleRate()
        {
            try
            {
                double length = mediaPlayer.currentMedia.duration;
                long frames = wave.SampleCount;
                return (int)(frames / length);
            }
            catch
            {
                return 0;
            }
        }
    }
}
