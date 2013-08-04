using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SpyCamcorder.Resources;

namespace SpyCamcorder
{
    public partial class Instructions : PhoneApplicationPage, INotifyPropertyChanged
    {
        public Instructions()
        {
            InitializeComponent();
            DataContext = this;
            ListOfInstructions = new ObservableCollection<Instruction>();

            LoadInstructions();
        }

        private void LoadInstructions()
        {
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction-1.jpg", AppResources.Instruction1, AppResources.Step1));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction0.jpg", AppResources.Instruction2, AppResources.Step2));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction1.jpg", AppResources.Instruction3, AppResources.Step3));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction2.jpg", AppResources.Instruction4, AppResources.Step4));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction3.jpg", AppResources.Instruction5, AppResources.Step5));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction4.jpg", AppResources.Instruction6, AppResources.Step6));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction5.jpg", AppResources.Instruction7, AppResources.Step7));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction6.jpg", AppResources.Instruction8, AppResources.Step8));
            ListOfInstructions.Add(new Instruction("InstructionImages/Instruction7.jpg", AppResources.Instruction9, AppResources.Step9));
        }

        private ObservableCollection<Instruction> _listOfInstructions;
        public ObservableCollection<Instruction> ListOfInstructions
        {
            get { return _listOfInstructions; }
            set { _listOfInstructions = value; RaisePropertyChanged("ListOfInstructions"); }
        }

        #region propertychanged

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion propertychanged
    }

    public class Instruction : INotifyPropertyChanged
    {
        public Instruction(string imageSource, string text, string header)
        {
            ImageSource = imageSource;
            Text = text;
            Header = header;
        }

        private string _imageSource;
        public string ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; RaisePropertyChanged("ImageSource"); }
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; RaisePropertyChanged("Text"); }
        }

        private string _header;
        public string Header
        {
            get { return _header; }
            set { _header = value; RaisePropertyChanged("Header"); }
        }
        

        #region propertychanged

        public void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion propertychanged
    }
}