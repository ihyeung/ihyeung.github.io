using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 namespace LMS.Controllers
{
  [Authorize(Roles = "Student")]
  public class StudentController : CommonController
  {
     public IActionResult Index()
    {
      return View();
    }
     public IActionResult Catalog()
    {
      return View();
    }
     public IActionResult Class(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }
     public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }
     public IActionResult ClassListings(string subject, string num)
    {
      System.Diagnostics.Debug.WriteLine(subject + num);
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      return View();
    }
     /*******Begin code to modify********/
     /// <summary>
    /// Returns a JSON array of the classes the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 6016)
    /// "name" - The course name
    /// "season" - The season part of the semester
    /// "year" - The year part of the semester
    /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
            var query = from en in db.Enrollment
                        join cl in db.Class
                        on en.ClassId equals cl.ClassId into classEnrollments
                        where uid.Equals(en.StudentId)
                         from ce in classEnrollments.DefaultIfEmpty()
                        join course in db.Course
                        on ce.CourseId equals course.CourseId
                        select new
                        {
                            subject = course.DeptId,
                            number = course.CourseNum,
                            name = course.Name,
                            season = ce.Semester,
                            year = ce.year,
                            grade = (en == null ? "--" : en.Grade)
                        };
             return Json(query.ToArray());
    }
     /// <summary>
    /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The category name that the assignment belongs to
    /// "due" - The due Date/Time
    /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="uid"></param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
    {
            int classId = lookupClassId(subject, num, season, year);
            System.Diagnostics.Debug.Assert(classId >= 0);
             var query = from en in db.Enrollment
                        join klass in db.Class
                        on en.ClassId equals klass.ClassId into classAssignments0
                        where en.StudentId.Equals(uid) //Filter by uid
                         from ca0 in classAssignments0.DefaultIfEmpty()
                        join ass in db.Assignment
                        on ca0.ClassId equals ass.ClassId into classAssignments1
                        where ca0.ClassId.Equals(classId) //Filter also by classId

                        from ca2 in classAssignments1.DefaultIfEmpty()
                        join sub in db.Submission
                        on ca2.AssignId equals sub.AssignId into classAssignments3

                        from ca3 in classAssignments3.DefaultIfEmpty()
                        select new
                        {
                            aname = ca2.Name,
                            cname = (from c in db.Assignment //Nested linq query since assignmentCategory is unrelated to other table joins
                                     join cat in db.AssignmentCategory
                                     on c.AssignType equals cat.CatId
                                     where c.Name.Equals(ca2.Name)
                                     select cat.Name),
                            due = ca2.Due,
                            score = ca3.NumPoints.ToString() ?? "null"
                        };
             return Json(query.ToArray());
    }
         /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year, string category, string asgname, string uid, string contents)
    {
            int classId = lookupClassId(subject, num, season, year);
            int assignmentId = lookupAssignmentId(classId, category, asgname);
            //System.Diagnostics.Debug.Assert(classId >= 0 && assignmentId >= 0);
            Submission s = new Submission();
            s.TextSub = contents;
            s.AssignId = assignmentId;
            s.NumPoints = 0;
            s.UserId = uid;
            s.SubmitTime = DateTime.Now;

            db.Submission.Add(s);
            db.SaveChanges();
       return Json(new { success = true });
    }
         /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. False if the student is already enrolled in the class.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            int classId = lookupClassId(subject, num, season, year);
            Enrollment e = new Enrollment(uid, classId, "--");
            const int MAX_ENROLL_CAPACITY = 500; //Updated to 500 to essentially remove this limitation
            if (!isValidEnrollment(e, season, year, uid, MAX_ENROLL_CAPACITY))
            {
                System.Diagnostics.Debug.WriteLine("Error encountered when enrolling in class. " +
                    "Either enrollment attempt outside registration window, enrollment already exists for this class," +
                    "or class is filled.");
                return Json(new { success = false });
            }
             db.Enrollment.Add(e);
            db.SaveChanges();
            //System.Diagnostics.Debug.WriteLine("Enrollment successful: {0} successfully enrolled in classId {1}", uid, classId);
            return Json(new { success = true });
        }
         private bool isValidEnrollment(Enrollment e, string semester, int year, string uid, int maxCapacity)
        {
            if (e.ClassId < 0) { return false; } //Invalid classId
            int query0 = db.Enrollment.Where((enrolled) => enrolled.ClassId == e.ClassId).Count();
            if (query0 >= maxCapacity) { return false; }
             var query = from en in db.Enrollment
                        where en.ClassId == e.ClassId
                        where en.StudentId.Equals(uid)
                        select en.StudentId;
            if (query.Any()) //Student can only enroll in a class once
            {
                System.Diagnostics.Debug.WriteLine("STUDENT IS ALREADY ENROLLED IN THIS CLASS");
                return false;
            }
            return enrollmentWithinRegistrationWindow(semester, year);
        }
         private bool enrollmentWithinRegistrationWindow(string semester, int year)
        {
            const int REG_WINDOW_DAYS = 100000; // You can enroll within 30 days of the semester start date
            //Updated to 100000 to essentially remove this date restriction on enrollment
            DateTime FALL_SEM_DAY_0 = new DateTime(year, 8, 20);
            DateTime SPRING_SEM_DAY_0 = new DateTime(year, 1, 7);
            DateTime SUMMER_SEM_DAY_0 = new DateTime(year, 5, 14);
            DateTime firstDay = semester.Equals("FALL") ? FALL_SEM_DAY_0 :
                semester.Equals("SPRING") ? SPRING_SEM_DAY_0 : SUMMER_SEM_DAY_0;
            DateTime curr = DateTime.Today;
             return (int)Math.Abs((curr - firstDay).TotalDays) <= REG_WINDOW_DAYS;
        }
         /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// Otherwise, the point-value of a letter grade for the UofU is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = from co in db.Course
                        join cl in db.Class
                        on co.CourseId equals cl.ClassId into courses
                         from c in courses.DefaultIfEmpty()
                        join en in db.Enrollment
                        on c.ClassId equals en.ClassId
                        where en.StudentId.Equals(uid)
                         select new
                        {
                            courseId = co.CourseId,
                            numCredits = co.numCredits,
                            grade = en.Grade
                        };
            int totalCreditHours = 0;
            double totalGpaPoints = 0;
            foreach (var v in query)
            {
                System.Diagnostics.Debug.WriteLine(v);
                if (v.grade == null || v.grade.Equals("--")) {
                    continue;
                }
                totalCreditHours += v.numCredits;
                totalGpaPoints += gpaLetterGradeToNumberMapper(v.grade) * v.numCredits;
            }
            System.Diagnostics.Debug.WriteLine("Total Credits: {0} \tTotal Grade Point Hours Earned: {1} \tCumulative GPA:{2}", totalCreditHours, totalGpaPoints, totalGpaPoints / totalCreditHours);
            return Json( new { gpa = totalGpaPoints / totalCreditHours });
        }
         private double gpaLetterGradeToNumberMapper(string grade)
        {
            double startVal = 4.0;
            double endVal = 0;
            ICollection<KeyValuePair<String, double>> mapper =
                                     new Dictionary<String, double>();
             mapper.Add(new KeyValuePair<String, double>("A", startVal));
            mapper.Add(new KeyValuePair<String, double>("A-", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("B+", startVal -= 0.4));
            mapper.Add(new KeyValuePair<String, double>("B", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("B-", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("C+", startVal -= 0.4));
            mapper.Add(new KeyValuePair<String, double>("C", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("C-", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("D+", startVal -= 0.4));
            mapper.Add(new KeyValuePair<String, double>("D", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("D-", startVal -= 0.3));
            mapper.Add(new KeyValuePair<String, double>("E", endVal));

            return mapper.Where(x => x.Key.Equals(grade)).First().Value;
        }
    }
} 
