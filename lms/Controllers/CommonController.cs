using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        protected Team10Context db;
        public CommonController()
        {
            //Initialize db context once in constructor
            db = new Team10Context();
        }
        /*
        * WARNING: This is the quick and easy way to make the controller
        *          use a different Context - good enough for our purposes.
        *          The "right" way is through Dependency Injection via the constructor (look this up if interested).
       */
        // TODO: Add a "UseContext" method if you wish to change the "db" context for unit testing
        //       See the lecture on testing
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        /*******Begin code to modify********/
        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query = from d in db.Department
                        select new
                        {
                            name = d.Name,
                            subject = d.DeptAbbrev
                        };
            return Json(query.ToArray());
        }
        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 6016)
        ///            "cname": The course name (e.g. "Database Systems and Applications")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query = from course in db.Course
                        group course by course.DeptId into classesByDept
                        join dept in db.Department
                        on classesByDept.FirstOrDefault().DeptId equals dept.DeptAbbrev
                        from cc in classesByDept
                        select new
                        {
                            subject = classesByDept.FirstOrDefault().DeptId,
                            dname = classesByDept.FirstOrDefault().Dept.Name,
                            courses = (from klass in classesByDept
                                       select new
                                       {
                                           number = klass.CourseNum,
                                           cname = klass.Name
                                       }).ToArray()
                        };
            return Json(query.ToArray().GroupBy(x => x.subject).Select(y => y.First())); //.First()/.GroupBy() to prevent duplicate department listings
        }
        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var query = from co in db.Course
                        join klass in db.Class
                        on co.CourseId equals klass.CourseId into CourseOfferings
                        where co.CourseNum == number
                        where co.DeptId.Equals(subject)
                        from cc in CourseOfferings
                        join prof in db.Professor
                        on cc.ProfUId equals prof.UId
                        select new
                        {
                            season = cc.Semester,
                            year = cc.year,
                            location = cc.Location == null ? "TBD" : cc.Location,
                            start = formatTimeSpan(cc.start_time),
                            end = formatTimeSpan(cc.end_time),
                            fname = prof.FirstName,
                            lname = prof.LastName
                        };
            return Json(query.ToArray());
        }
        /// <summary>
        /// Helper function that formats time of type TimeSpan into a string
        /// of the format hh:mm:ss.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private String formatTimeSpan(TimeSpan? time)
        {
            return String.Format("{0:0}:{1:00}:{2:00}", time.Value.Hours,
                            time.Value.Minutes, time.Value.Seconds);
        }
        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            int classId = lookupClassId(subject, num, season, year);
            int assignmentId = lookupAssignmentId(classId, category, asgname);
            System.Diagnostics.Debug.Assert(classId >= 0 && assignmentId >= 0);
            var query = from ass in db.Assignment
                        where ass.AssignId == assignmentId
                        select ass.Contents;
            return Content(query.First());
        }
        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            int assignmentId = lookupAssignmentId(lookupClassId(subject, num, season, year), category, asgname);
            var query = from ass in db.Assignment
                        join submission in db.Submission
                        on ass.AssignId equals submission.AssignId
                        where submission.UserId.Equals(uid)
                        select submission.TextSub;
            return !query.Any() ? Content("") : Content(query.First());
        }
        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user.
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>The user JSON object or an object containing {success: false} if the user doesn't exist</returns>
        public IActionResult GetUser(string uid)
        {
            return lookupAdministrator(uid) ?? lookupProfessor(uid) ??
                lookupStudent(uid) ?? Json(new { success = false });
        }
        /******** HELPER METHODS ***********/

        private IActionResult lookupStudent(string uid)
        {
            var query = db.Student.Where(x => x.UId == uid)
                .Select(x => new { fname = x.FirstName, lname = x.LastName, uid = uid, department = x.Dept.Name });
            return !query.Any() ? null : Json(query.Single());
        }
        private JsonResult lookupProfessor(string uid)
        {
            var query = db.Professor.Where(x => x.UId == uid)
                .Select(x => new { fname = x.FirstName, lname = x.LastName, uid = uid, department = x.Dept.Name });
            return !query.Any() ? null : Json(query.Single());
        }
        private JsonResult lookupAdministrator(string uid)
        {
            var query = db.Administrator.Where(x => x.UId == uid)
                .Select(x => new { fname = x.FirstName, lname = x.LastName, uid = uid });
            return !query.Any() ? null : Json(query.Single());
        }
        /// <summary>
        /// Helper method that looks up unique class id identifier.
        /// Error return values:
        /// returns -1 if empty JArray (invalid classId),
        /// returns really large negative number if duplicate classIds found (this should never happen).
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="num"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        public int lookupClassId(string subject, int num, string season, int year)
        {
            var query = from cl in db.Class
                        join co in db.Course
                        on cl.CourseId equals co.CourseId
                        where cl.Semester.Equals(season)
                        where cl.year == year
                        where co.DeptId.Equals(subject)
                        where co.CourseNum == num
                        select cl.ClassId;
            ////Only way i could figure out how to serialize a JSON object in a form that could be demarshalled later
            //var arr = Newtonsoft.Json.Linq.JArray.FromObject(query.ToArray());
            //// to get value of "classId" JToken access 0th index in JArray, then look up JToken by key name
            //return arr.Count() > 0 ? arr.Count() == 1 ? (int)arr[0]["classId"] : Int32.MinValue : -1;
            return query.Any() ?
                   query.Count() == 1 ? (int)query.First() : Int32.MinValue : -1;
        }
        /// <summary>
        /// Helper function to look up unique assignmentId.
        /// Error return values:
        /// returns -1 if empty JArray (invalid classId),
        /// returns really large negative number if duplicate classIds found (this should never happen).
        /// </summary>
        /// <param name="classId"></param>
        /// <param name="category"></param>
        /// <param name="asgname"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public int lookupAssignmentId(int classId, string category, string asgname)
        {
            using (Team10Context db = new Team10Context()) //Initialized instance of db context here to resolve dbcontext multithreading issues
            {
                if (classId < 0)
                {
                    return -1;
                }

                var query = from cl in db.Class
                            join ass in db.Assignment
                            on cl.ClassId equals ass.ClassId
                            where ass.Name.Equals(asgname)
                            select ass.AssignId;

                return query.Any() ?
                    query.Count() == 1 ? (int)query.First() : Int32.MinValue : -1;
            }
        }
        public int lookupCourseId(string dept, int num)
        {
            var query = from co in db.Course
                        where co.DeptId.Equals(dept)
                        where co.CourseNum.Equals(num)
                        select co.CourseId;
            return query.Any() ?
                   query.Count() == 1 ? (int)query.First() : Int32.MinValue : -1;
        }
        public string formatDateTime(DateTimeOffset? date)
        {
            return String.Format("{0} {1}/{2}/{3} {4:0}:{5:00} {6} ", date.Value.DayOfWeek,
                            date.Value.Month, date.Value.Day,
                            date.Value.Year, date.Value.Hour % 12, date.Value.Minute,
                            (date.Value.Hour >= 12 ? "PM" : "AM"));
        }
    }

    /**** END OF HELPER METHODS ****** /
    /*******End code to modify********/
}
