using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using School.Data;


namespace School
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Student student;
        // Connection to the School database
        private SchoolDBEntities schoolContext = null;

        // Field for tracking the currently selected teacher
        private Teacher teacher = null;

        // List for tracking the students assigned to the teacher's class
        private IList studentsInfo = null;

        #region MyMethods

        private void editStudent(Student student)
        {
            StudentForm sf = new StudentForm();

            sf.Title = "Edit Student Details";
            sf.firstName.Text = student.FirstName;
            sf.lastName.Text = student.LastName;
            sf.dateOfBirth.Text = student.DateOfBirth.ToString("d"); // Format the date to omit the time element

            if (sf.ShowDialog().Value==true)
            {
                student.FirstName = sf.firstName.Text;
                student.LastName = sf.lastName.Text;
                student.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text);
                saveChanges.IsEnabled = true;
            }
        }

        private void addNewStudent()
        {
            StudentForm sf = new StudentForm();
            sf.Title = "New Student for Class " + teacher.Class;
            if (sf.ShowDialog().Value)
            {
                Student newStudent = new Student();
                newStudent.FirstName = sf.firstName.Text;
                newStudent.LastName = sf.lastName.Text;
                newStudent.DateOfBirth = DateTime.Parse(sf.dateOfBirth.Text);
                this.teacher.Students.Add(newStudent);
                this.studentsInfo.Add(newStudent);
                saveChanges.IsEnabled = true;
            }
        }

        private void removeStudent(Student student)
        {
            MessageBoxResult response = MessageBox.Show(
            String.Format("Remove {0}", student.FirstName + " " + student.LastName),
            "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No);

            // If the user clicked Yes, remove the student from the database
            if (response == MessageBoxResult.Yes)
            {
                this.schoolContext.Students.DeleteObject(student);

                // Enable saving (changes are not made permanent until they are written back to the database)
                saveChanges.IsEnabled = true;
            }
        }

        #endregion

        #region Predefined code

        public MainWindow()
        {
            InitializeComponent();
        }

        // Connect to the database and display the list of teachers when the window appears
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.schoolContext = new SchoolDBEntities();
            teachersList.DataContext = this.schoolContext.Teachers;
        }

        // When the user selects a different teacher, fetch and display the students for that teacher
        private void teachersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Find the teacher that has been selected
            this.teacher = teachersList.SelectedItem as Teacher;
            this.schoolContext.LoadProperty<Teacher>(this.teacher, s => s.Students);

            // Find the students for this teacher
            this.studentsInfo = ((IListSource)teacher.Students).GetList();

            // Use databinding to display these students
            studentsList.DataContext = this.studentsInfo;
        }

        #endregion

        // When the user presses a key, determine whether to add a new student to a class, remove a student from a class, or modify the details of a student
        private void studentsList_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                // If the user pressed Enter, edit the details for the currently selected student
                case Key.Enter: Student student = this.studentsList.SelectedItem as Student;
                    editStudent(student);
                    break;

                // If the user pressed Insert, add a new student
                case Key.Insert: Student studentadd = this.studentsList.SelectedItem as Student;
                    addNewStudent();
                    break;

                // If the user pressed Delete, remove the currently selected student
                case Key.Delete: student = this.studentsList.SelectedItem as Student;
                    removeStudent(student);
                    break;
            }
        }

        // TODO: Exercise 1: Task 1a: If the user double-clicks a student, edit the details for that student
        private void studentsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Student student = this.studentsList.SelectedItem as Student;
            editStudent(student);
            
        }

        // Save changes back to the database and make them permanent
        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }

    [ValueConversion(typeof(string), typeof(Decimal))]
    class AgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            // Convert the date of birth provided in the value parameter and convert to the age of the student in years
            if (value != null)
            {
                DateTime studentDateOfBirth = (DateTime)value;
                TimeSpan difference = DateTime.Now.Subtract(studentDateOfBirth);
                int ageInYears = (int)(difference.Days / 365.25);
                return ageInYears.ToString();
            }
            else
            {
                return "";
            }
        }

        #region Predefined code

        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
