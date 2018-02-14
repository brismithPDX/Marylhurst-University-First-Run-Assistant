using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.DirectoryServices.AccountManagement;

namespace MHU_First_Run_Assistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Magic Numbers and other bad ideas
        private int _MAX_ROOM_NO_LENGTH = 5;
        private int _errors = 0;
        private bool _ROOMerrors = false;

        public MainWindow()
        {

            InitializeComponent();
            
        }

        private void Cancel(object sender, RoutedEventArgs e) 
        {
            System.Windows.Application.Current.Shutdown();
        }                            // Cancel Button Action

        private void Continue(object sender, RoutedEventArgs e)
        {
            Validate_And_SetErrorBorders();

            if ((_errors > 0) || _ROOMerrors)
            {
                MessageBox.Show("Error: Please resolve the above problems before continuing.", "ERROR");

            }
            else
            {
                  
                //move the computer in AD and update the machine description per the user defined department information
                ADInterface MoveInterface = new ADInterface();
                MoveInterface.MoveOrgUnit(Department_Box.Text, "LDAP://OU=Employee Computers, OU=Marylhurst Computers, DC=campus, DC=marylhurst, DC=edu"); //Departnment_Box.Text is refering to the content on the form labled department box, this works because the user control is named.
                MoveInterface.UpdateDescription(Building_Box.Text, Room_Box.Text, Computer_Type.Text);

                LocalSystemInterface localSystemInterface = new LocalSystemInterface();
                localSystemInterface.UpdatePolicy();

                MessageBox.Show("Computer policy configuration and invetory compleate. \n Please enjoy your new computer", "SUCCESS");

                System.Windows.Application.Current.Shutdown();
            }

        }                           // Continue Button Action

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            //Create List of departments to show in department ComboBox
            List<String> Departments = new List<string> { "Select Department" };

            //Add AD Departments to list
            ADInterface SearchInterface = new ADInterface();
            //create required list to eliminate forbidden OU's from user shown list
            List<string> BadOUs = new List<string> { "Temp", "Test OU", "TST - Test Computers", "Sitecode Change", "Staff Exceptions"};
            Departments = SearchInterface.GetOrgUnits("LDAP://OU=Employee Computers, OU=Marylhurst Computers, DC=campus, DC=marylhurst, DC=edu", BadOUs);
            
            //make list alphabetical
            Departments.Sort();
            //Add inital prefix item for list display
            Departments.Insert(0, "Select Department");
            

            //Update box Items with new departments list
            var this_box = sender as ComboBox;
            this_box.ItemsSource = Departments;
            this_box.SelectedIndex = 0;
        }                    // Department Box Loading Actions

        private void Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Validate combobox input at selection time
            var this_box = sender as ComboBox;
            if(this_box.SelectedIndex == 0)
            {
                this_box.BorderBrush = System.Windows.Media.Brushes.Red;
                _errors++;

            }
            else
            {
                _errors--;
                this_box.BorderBrush = System.Windows.Media.Brushes.Gray;
            }

        }   // Department Box Selection Change Actions - Performs red error border activation

        private void Room_Box_Validate()
        {
            //validate room selection at input time
            
            if(Room_Box.Text.Length == 0)
            {
                RoomBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                _ROOMerrors = true;
            }
            if (Room_Box.Text.Length > _MAX_ROOM_NO_LENGTH)
            {
                RoomBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                _ROOMerrors = true;
            }
            else
            {
                _ROOMerrors = false;
                RoomBorder.BorderBrush = System.Windows.Media.Brushes.Gray;

            }

        }          // Room Department Box Selection Change Actions - Performs red error border activation

        private void Room_Box_GotFocus(object sender, RoutedEventArgs e)
        {
            var this_textbox = sender as TextBox;
            if(this_textbox.Text == "room number")
            {
                this_textbox.Text = "";
            }
        }                  // Room Number Box Slected Focus Actions - Clears defualt "Room Number" text from box if needed

        private void Validate_And_SetErrorBorders()
        {

            // Check building options are Valid
            if(Building_Box.SelectedIndex == 0)
            {
                BuildingBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                _errors++;
            }
            else
            {
                BuildingBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
                _errors--;
            }

            // Check Department options are Valid
            if(Department_Box.SelectedIndex == 0)
            {
                DepartmentBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                _errors++;
            }
            else
            {
                DepartmentBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
                _errors--;
            }

            // Check Device Type options are Valid

            if(Computer_Type.SelectedIndex == 0)
            {
                TypeBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                _errors++;
            }
            else
            {
                TypeBorder.BorderBrush = System.Windows.Media.Brushes.Gray;
                _errors--;
            }

            Room_Box_Validate();

        }                                       // Validates Page content and sets error borders if needed.

    }
}
