using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;
using System.Drawing;

namespace WpfApplication4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        List<double> imageIntensity = new List<double>();
        //New hashtable to store intensity of each image
        
        string selecteImageFilePath = null;       
        int pageLength = 20;
        int pageIndexMain = 0;      
        int maxPage = 0;
        int fileLength=0;
        List<string> files;
        List<double> eachImagePixelValue = new List<double>();
        List<HistogramBin> allimageDetailIntensityMethod = new List<HistogramBin>();
        //List<HistogramBin> allimageDetailColorCodeMethod = new List<HistogramBin>();
        List<string> intensityResultFiles = new List<string>();
        List<string> colorCodeResultFiles = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            readDataBase();
        }
        //Read the database.
        public void readDataBase()
        {
            string path = Environment.CurrentDirectory + "/" + "images";
            // C:\Users\Funapp\Downloads\images\images"
            files = new List<string>(Directory.GetFiles(path, "*.jpg", SearchOption.AllDirectories));    
            fileLength = files.Count();
            maxPage = fileLength / pageLength;
            renderImage(files);
            //createImageHistogramBin(files);
            computePixelValue(files);
        }
        //Display image in UI
        public void renderImage(List<string> files)
        {
            try
            {
                int indexType = this.pageIndexMain;
                List<string> imagePerPage = computeImagePerPage(pageLength, indexType, files);
                //Add images og only one page 
                List<ImageDetails> imagelist = new List<ImageDetails>();
                //foreach (string filename in files)
                foreach (string filename in imagePerPage)
                {
                    //Bitmap b = new Bitmap(filename,true );
                    //PixelFormat pf = PixelFormats.Argb32;
                    ImageDetails img = new ImageDetails();
                    img.image = new Uri(filename);
                    imagelist.Add(img);
                }

                disp.ItemsSource = imagelist;
                pageNumber.Content = "Page" + (indexType + 1) + "of" + maxPage;
            }
            
            catch (Exception e)
            {

            }
        }
        //Selected image to display
        private void imageselected_click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            //HistogramBin histogramBinIntensity = new HistogramBin();
            //histogramBinIntensity.initializeHistogramIntensityMethod();
            System.Windows.Controls.Image buttonImage = button.Content as System.Windows.Controls.Image; //Cntrl+alt+q (During debugging)
            if (buttonImage == null ||
               buttonImage.Source == null)
            {
                throw new Exception("bug!!");
            }
            string imagePath = buttonImage.Source.ToString();
            selecteImageFilePath = imagePath;
            //selectImage.Source = new Bitmap(imagePath,true);
            //new System.Windows.Media.Imaging.BitmapImage()
            selectImage.Source = new System.Windows.Media.Imaging.BitmapImage(
            new Uri(imagePath, UriKind.Absolute));

            if(this.IntensityMethod.IsChecked.GetValueOrDefault())
            {
                this.intensityMethod_click(null, null);
            }

            if( this.ColorCodeMethod.IsChecked.GetValueOrDefault())
            {
                this.colorCodeMethod_click(null, null);
            }
        }
        //Compute the pixel value and add it in histogram bin
        private void computePixelValue(List<string> files)
        {
            foreach (string filepath in files)
            {
                HistogramBin histogramBinIntensity = new HistogramBin();
                histogramBinIntensity.initializeHistogramIntensityMethod();
                histogramBinIntensity.initializehistogramColorCodeMethod();
                histogramBinIntensity.setImageId(filepath);

                Bitmap b = new Bitmap(filepath, true);
                int width =b.Width;
                int height = b.Height;
                int totalPixel = height * width;
                histogramBinIntensity.setNumberOfPixelImage(totalPixel);
                for(int i=0;i<width;i++)
                {
                    for(int j=0;j<height;j++)
                    {
                        Color pixel = b.GetPixel(i, j);
                        byte red =pixel.R;
                        byte green = pixel.G;
                        byte blue = pixel.B;

                        //Colorcode method
                        int colorCode = (blue & (192)) >> 6 | (green & (192)) >> 4 | (red & (192)) >> 2;
                        histogramBinIntensity.addElementHistogramColorCodeMethod(colorCode);
                        //Intensity method
                        double I = 0.299 * red + 0.587 * green + 0.114 * blue;
                        histogramBinIntensity.addIntensityToBin(I);
                    }
                }
                allimageDetailIntensityMethod.Add(histogramBinIntensity);
            }
        }
        //Click Intensity method radio button
        private void intensityMethod_click(object sender, RoutedEventArgs e)
        {
            string intensity = "intensity";
            this.setPageIndex(0);
            //pageIndexIntensity = 0;
            //pageIndexColorCode = 0;
            intensityResultFiles.Clear();
            ManhattanDistance distance = new ManhattanDistance();
            if(selecteImageFilePath==null)
            {
                MessageBox.Show("Please select one Image before we proceed");
                return;
            }
            List<ResultCBIR> sortedDistance = distance.computeManhattanDistance(selecteImageFilePath,
                    allimageDetailIntensityMethod,intensity);
            foreach (ResultCBIR fieldid in sortedDistance)
            {
                intensityResultFiles.Add(fieldid.imageid);
            }
            renderImage(intensityResultFiles);
        }
        //Click color code method button
        private void colorCodeMethod_click(object sender, RoutedEventArgs e)
        {
            
                string colorcode = "colorcode";
                setPageIndex(0);
                colorCodeResultFiles.Clear();
                ManhattanDistance distance = new ManhattanDistance();
                if (selecteImageFilePath == null)
                {
                    MessageBox.Show("Please select one Image before we proceed");
                    return;
                }
                List<ResultCBIR> sortedDistance = distance.computeManhattanDistance(selecteImageFilePath,
                        allimageDetailIntensityMethod, colorcode);
                foreach (ResultCBIR fieldid in sortedDistance)
                {
                    colorCodeResultFiles.Add(fieldid.imageid);
                }
                renderImage(colorCodeResultFiles);
            
        }

        /*Code below Display the images in pages */
        public List<string> computeImagePerPage(int pageLength, int pageIndex, List<string> files)
        {
            List<string> ImagePerPage = new List<string>();
            List<string> EmptyImagePerPage = new List<string>();
            int startIndex = 0;
            int endIndex = 0;
            if (startIndex >= 0 && endIndex < fileLength)
            {
                startIndex = pageIndex * pageLength;
                endIndex = startIndex + pageLength - 1;
                for (int i = startIndex; i <= endIndex; i++)
                {
                    ImagePerPage.Add(files.ElementAt(i));
                }
                return ImagePerPage;
            }
            else
            {
                return EmptyImagePerPage;
            }
        }


        //Method the detect which file to display 
        public List<string> cbirMethodType()
        {
            if (IntensityMethod.IsChecked == true)
            {
                return intensityResultFiles;
            }
            else if (ColorCodeMethod.IsChecked == true)
            {
                return colorCodeResultFiles;
            }
            else
            {
                return files;
            }
        }

        //Previous page button click event
        private void previousPage_Click(object sender, RoutedEventArgs e)
        {
            Button buttonPrev = sender as Button;
            int pageIndex = this.pageIndexMain;
            this.setPageIndex(--pageIndex);
            List<string> files = cbirMethodType();
            renderImage(files);
        }

        public void setPageIndex(int index)
        {
            if (index < 0 || index >= this.maxPage)
            {
                return;
            }

            this.pageIndexMain = index;

            if (this.pageIndexMain == 0)
            {
                this.Previouspage.IsEnabled = false;
            }
            else
            {
                this.Previouspage.IsEnabled = true;
            }

            if (this.pageIndexMain == this.maxPage - 1)
            {
                this.Nextpage.IsEnabled = false;
            }
            else
            {
                this.Nextpage.IsEnabled = true;
            }
        }

        //Next page butoon click event
        private void nextPage_click(object sender, RoutedEventArgs e)
        {   
            Button buttonNext = sender as Button;
            int pageIndex = this.pageIndexMain;
            this.setPageIndex(++pageIndex);
            List<string> files = cbirMethodType();
            renderImage(files);
           
        }

        public bool checkIntermediateStateOfPage(int pageIndex)
        {
            if(pageIndex>0 && pageIndex<maxPage)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void reset_click(object sender, RoutedEventArgs e)
        {
            this.setPageIndex(0);
            renderImage(files);
        }
    }
}
