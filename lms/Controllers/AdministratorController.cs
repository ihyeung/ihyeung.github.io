using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 namespace LMS.Controllers
{
  [Authorize(Roles = "Administrator")]
  public class AdministratorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }
     public IActionResult Department(string subject)
    {
      ViewData["subject"] = subject;
      return View();
    }
     public IActionResult Course(string subject, string num)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }
     /// <summary>
    /// Returns a JSON array of all the courses in the given department.
    /// Each object in the array should have the following fields:
    /// "number" - The course number (as in 6016 for this course)
    /// "name" - The course name (as in "Database Systems..." for this course)
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <returns></returns>
    public IActionResult GetCourses(string subject)
    {
            var query = from courses in db.Course
                        join dept in db.Department on courses.DeptId equals dept.DeptAbbrev
                        where courses.DeptId.Equals(subject)
                        select new
                        {
                            number = courses.CourseNum,
                            name = courses.Name
                        };
             return Json(query.ToArray());
     }
     /// <summary>
    /// Returns a JSON array of all the professors working in a given department.
    /// Each object in the array should have the following fields:
    /// "lname" - The professor's last name
    /// "fname" - The professor's first name
    /// "uid" - The professor's uid
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <returns></returns>
    public IActionResult GetProfessors(string subject)
    {
            var query = from dept in db.Department
                        join prof in db.Professor on dept.DeptAbbrev equals prof.DeptId
                        where dept.DeptAbbrev.Equals(subject)
                        select new
                        {
                            lname = prof.LastName,
                            fname = prof.FirstName,
                            uid = prof.UId
                        };
             return Json(query.ToArray());
        }
         /// <summary>
        /// Creates a course.
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}. False if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
    {
            Course course = new Course();
            course.DeptId = subject;
            course.CourseNum = number;
            course.Name = name;
            course.numCredits = 4;
            var query = from c in db.Course
                        where c.DeptId.Equals(subject)
                        where c.CourseNum.Equals(number)
                        select c.Name;
            if (query.Any())
            {
                return Json(new { success = false });
            }
            else
            {
                db.Course.Add(course);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }
     /// <summary>
    /// Creates a class offering of a given course.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="number">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="start">The start time</param>
    /// <param name="end">The end time</param>
    /// <param name="location">The location</param>
    /// <param name="instructor">The uid of the professor</param>
    /// <returns>A JSON object containing {success = true/false}. False if another class occupies the same location during any time within the start-end range in the same semester.</returns>
    public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
    {
             var query = from kl in db.Class
                        where kl.Semester.Equals(season)
                        where kl.year == year
                        select new
                        {
                            classId = kl.ClassId,
                            start = kl.start_time,
                            end = kl.end_time,
                            location = location,
                            prof = instructor
                        };
           Func<DateTime, DateTime, TimeSpan?, TimeSpan?, bool> timeConflict = delegate (DateTime s, DateTime e, TimeSpan? start1, TimeSpan? end1)
          {
              return e.TimeOfDay > start1 && s.TimeOfDay < end1 ||
              (end1 > s.TimeOfDay && start1 < e.TimeOfDay) ||
              (s.TimeOfDay == start1 && e.TimeOfDay == end1);
          };
             var conflicts = query.Where(c =>
            (c.location.Equals(location) && timeConflict(start, end, c.start, c.end) ||
            (timeConflict(start, end, c.start, c.end) && c.prof.Equals(instructor))));
            if (conflicts.Any())//Either classroom is filled or professor is teaching another class at this time
            {
                return Json(new { success = false });
}
            else
            {
                Class cc = new Class();
                cc.CourseId = lookupCourseId(subject, number);
                System.Diagnostics.Debug.Assert(cc.CourseId >= 0);
                cc.Location = location;
                cc.start_time = start.TimeOfDay;
                cc.end_time = end.TimeOfDay;
                cc.Semester = season;
                cc.year = year;
                cc.ProfUId = instructor;
                db.Class.Add(cc);
                db.SaveChanges();
                return Json(new { success = true });
            }
    }
   }
} 
