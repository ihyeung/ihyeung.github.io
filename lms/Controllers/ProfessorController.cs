using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 namespace LMS.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : CommonController
    {
        public IActionResult Index()
        {
            return View();
        }
         public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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
         public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }
         public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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
         public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }
         public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }
         /*******Begin code to modify********/
         /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from cl in db.Class
                        join en in db.Enrollment
                        on cl.ClassId equals en.ClassId into classEnrollments
                        where cl.ClassId == lookupClassId(subject, num, season, year)
                         from e in classEnrollments.DefaultIfEmpty()
                        join st in db.Student
                        on e.StudentId equals st.UId
                        select new
                        {
                            fname = st.FirstName,
                            lname = st.LastName,
                            uid = st.UId,
                            dob = st.birth_date,
                            grade = e.Grade
                        };
             return Json(query.ToArray());
        }
         /// <summary>
        /// Assume that a specific class can not have two categories with the same name.
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
             int classId = lookupClassId(subject, num, season, year);
             var query = from c in db.Class
                        join ass in db.Assignment
                        on c.ClassId equals ass.ClassId into ClassAssignments
                        where c.ClassId.Equals(classId)
                         from ca in ClassAssignments.DefaultIfEmpty()
                        join ac in db.AssignmentCategory
                        on ca.AssignType equals ac.CatId
                        where (!string.IsNullOrEmpty(category) && ac.Name.Equals(category))
                        || (string.IsNullOrEmpty(category) && ac.Name != null)
                        select new
                        {
                            aname = ca.Name,
                            cname = ac.Name,
                            due = formatDateTime(ca.Due),
                            submissions = (from sub in db.Submission
                                           where sub.AssignId == lookupAssignmentId(classId, ac.Name, ca.Name)
                                           select sub).Count()
                        };

            return Json(query.ToArray());
        }
         /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            int classId = lookupClassId(subject, num, season, year);
            var query = from cat in db.AssignmentCategory
                        where cat.ClassId == classId
                        select new
                        {
                            name = cat.Name,
                            weight = cat.Weight
                        };
            return Json(query.ToArray());
        }
         /// <summary>
        /// Creates a new assignment category for the specified class.
        /// A class can not have two categories with the same name.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            int classId = lookupClassId(subject, num, season, year);
            var query = from co in db.Course
                        join cl in db.Class
                        on co.CourseId equals cl.CourseId into Courses
                         from c in Courses.DefaultIfEmpty()
                        join cat in db.AssignmentCategory
                        on c.ClassId equals cat.ClassId
                        select new
                        {
                            name = cat.Name,
                            weight = cat.Weight
                        };
            if (query.Any())
            {
                foreach (var result in query)
                {
                    if (result.name.Equals(category))
                    {
                        return Json(new { success = false });
                    }
                }
            }
            AssignmentCategory ac = new AssignmentCategory();
            ac.Name = category;
            ac.Weight = catweight;
            ac.ClassId = classId;
            db.AssignmentCategory.Add(ac);
            db.SaveChanges();
            return Json(new { success = true });
        }
         /// <summary>
        /// Creates a new assignment for the given class and category.
        /// An assignment category (which belongs to a class) can not have two assignments with
        /// the same name.
        /// If an assignment of the given category with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            int classId = lookupClassId(subject, num, season, year);
            var categoryQuery = from cat in db.AssignmentCategory
                              where cat.ClassId == classId
                              where cat.Name.Equals(category)
                              select cat.CatId;
            int categoryId = categoryQuery.Any() ? categoryQuery.First() : -1;
            var query = from a in db.Assignment
                        join cl in db.Class
                        on a.ClassId equals cl.ClassId into classAssignments
                        where a.ClassId == classId
                         from ca in classAssignments.DefaultIfEmpty()
                        join ac in db.AssignmentCategory
                        on ca.ClassId equals ac.ClassId
                        where ac.Name.Equals(category)
                        select a.Name;
            if (query.Contains(asgname))
            {
                return Json(new { success = false });
            }
             Assignment ass = new Assignment();
            ass.ClassId = classId;
            ass.Name = asgname;
            ass.MaxPoints = asgpoints;
            ass.Due = asgdue;
            ass.Contents = asgcontents;
            ass.AssignType = categoryId;
            db.Assignment.Add(ass);
            db.SaveChanges();
             var queryStudents = from enrolled in db.Enrollment
                                where enrolled.ClassId == classId
                                select enrolled.StudentId;
            foreach (var uid in queryStudents)
            {
                updateClassGrade(uid, classId);
            }
            return Json(new { success = true });
        }
         /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        ///
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
             int assignId = lookupAssignmentId(lookupClassId(subject, num, season, year), category, asgname);
            System.Diagnostics.Debug.Assert(assignId >= 0);
            var query = from ass in db.Assignment
                        join sub in db.Submission
                        on ass.AssignId equals sub.AssignId into assignmentSubmissions
                        where ass.AssignId == assignId
                         from subass in assignmentSubmissions.DefaultIfEmpty()
                        join st in db.Student
                        on subass.UserId equals st.UId
                        select new
                        {
                            fname = st.FirstName,
                            lname = st.LastName,
                            uid = st.UId,
                            time = formatDateTime(subass.SubmitTime),
                            score = subass.NumPoints
                        };
            return Json(query.ToArray());
        }
         /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            int classId = lookupClassId(subject, num, season, year);
            int assignId = lookupAssignmentId(classId, category, asgname);
            System.Diagnostics.Debug.Assert(assignId >= 0);
            var query = from sub in db.Submission
                        where sub.AssignId == assignId
                        where sub.UserId.Equals(uid)
                        select sub;
            if (!query.Any())
            {
                return Json(new { success = false });
            }
            query.First().NumPoints = Convert.ToDouble(score);
            updateClassGrade(uid, classId);
             return Json(new { success = true });
        }
         /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 6016)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Class
                        join co in db.Course
                        on c.CourseId equals co.CourseId
                        where c.ProfUId.Equals(uid)
                        select new
                        {
                            subject = co.DeptId,
                            number = co.CourseNum,
                            name = co.Name,
                            season = c.Semester,
                            year = c.year
                        };

            return Json(query.ToArray());
        }
         public void updateClassGrade(string uid, int classId)
        {
            var updateQuery = from en in db.Enrollment
                              where en.StudentId.Equals(uid)
                              where en.ClassId == classId
                              select en;
            if (updateQuery.Any())
            {
                updateQuery.First().Grade = calculateClassGrade(uid, classId);
                db.SaveChanges();
            }
        }
         public string calculateClassGrade(string uid, int classId)
        {
            var classCategories = from c in db.AssignmentCategory
                                  where c.ClassId == classId
                                  select c;
            double? total = 0;
            foreach (var category in classCategories)
            {
                double? rawPercent = categoryPercent(classId, category.Name, uid, category.Weight);
                total += rawPercent;
            }
            return percentToLetterGradeMapper(total * classScaleFactor(classId));
         }
         private double? categoryPercent(int classId, string category, string uid, int catWeight)
        {
            int? totalPoints = 0;
            double? totalScore = 0;
            var allAssignmentsForCategory = from ass in db.Assignment
                                            join cat in db.AssignmentCategory
                                            on ass.AssignType equals cat.CatId
                                            where ass.ClassId == classId
                                            where cat.Name.Equals(category)
                                            select ass;
            foreach (var assignment in allAssignmentsForCategory)
            {
                totalScore += getAssignmentScore(assignment.AssignId, uid);
                totalPoints += getAssignmentMaxScore(assignment.AssignId);
            }
            System.Diagnostics.Debug.Assert(totalScore <= totalPoints);
            return totalPoints == 0 ? 0 : (totalScore / totalPoints) * catWeight;
        }
         private double? classScaleFactor(int classId)
        {
            var query = from c in db.AssignmentCategory
                        where c.ClassId == classId
                        select c.Weight;
            int totalWeight = 0;
            foreach(var weight in query)
            {
                totalWeight += weight;
            }
            return totalWeight == 100 ? 1 : 100 / totalWeight;
        }
         private double? getAssignmentScore(int assignId, string uid)
        {
            var query = from sub in db.Submission
                        where sub.AssignId == assignId
                        where sub.UserId.Equals(uid)
                        select sub;
            return query.Any() ? query.First().NumPoints : 0;
        }
         private int? getAssignmentMaxScore(int assignId)
        {
            var query = from ass in db.Assignment
                        where ass.AssignId == assignId
                        select ass.MaxPoints;
            return query.Any() ? query.First() : 0;
        }
         private string percentToLetterGradeMapper(double? percent)
        {
            SortedDictionary<double, string> mapper = new SortedDictionary<double, string>();
             mapper.Add(93, "A");
            mapper.Add(90, "A-");
            mapper.Add(87, "B+");
            mapper.Add(83, "B");
            mapper.Add(80, "B-");
            mapper.Add(77, "C+");
            mapper.Add(73, "C");
            mapper.Add(70, "C-");
            mapper.Add(67, "D+");
            mapper.Add(63, "D");
            mapper.Add(60, "D-");
            mapper.Add(0, "E");
            return mapper.Where(x => percent <= x.Key).First().Value;
        }
    }
} 
