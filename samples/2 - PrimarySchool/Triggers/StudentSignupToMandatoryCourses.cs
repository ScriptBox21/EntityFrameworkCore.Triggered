﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EntityFrameworkCore.Triggered;

namespace PrimarySchool.Triggers
{
    public class StudentSignupToMandatoryCourses : IBeforeSaveTrigger<Student>
    {
        readonly ApplicationContext _applicationContext;

        public StudentSignupToMandatoryCourses(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public Task BeforeSave(ITriggerContext<Student> context, CancellationToken cancellationToken)
        {
            var mandatoryCourses = _applicationContext.Courses
                .Where(x => x.IsMandatory)
                .ToList();
            
            foreach (var mandatoryCourse in mandatoryCourses)
            {
                var studentRegistration = _applicationContext.StudentCourses.Find(context.Entity.Id, mandatoryCourse.Id);
                if (studentRegistration == null)
                {
                    _applicationContext.StudentCourses.Add(new StudentCourse { StudentId = context.Entity.Id, CourseId = mandatoryCourse.Id });
                }
            }

            return Task.CompletedTask;
        }
    }
}
