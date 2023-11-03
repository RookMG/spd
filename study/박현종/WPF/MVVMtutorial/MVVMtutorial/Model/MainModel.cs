using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMtutorial.Model
{
    public class MainModel
    {
        public string dollar;
        public string won;
    }

    public class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }

    public class StudentList : ObservableCollection<Student>
    {
        public StudentList()
        {
            Add(new Student() { Name = "안정환", Age = 20, Gender = "남" });
            Add(new Student() { Name = "아이유", Age = 38, Gender = "여" });
            Add(new Student() { Name = "정형돈", Age = 21, Gender = "남" });
            Add(new Student() { Name = "광희", Age = 21, Gender = "여" });
        }
    }
}
