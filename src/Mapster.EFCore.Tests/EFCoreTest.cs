using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapster.EFCore.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.EFCore.Tests
{
    [TestClass]
    public class EFCoreTest
    {
        [TestMethod]
        public void TestFindObject()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase($"School-{Guid.NewGuid()}")
                .Options;
            using var context = new SchoolContext(options);
            DbInitializer.Initialize(context);

            var dto = new StudentDto
            {
                ID = 7,
                Enrollments = new List<EnrollmentItemDto>
                {
                    new EnrollmentItemDto
                    {
                        EnrollmentID = 12,
                        Grade = Grade.F,
                    }
                }
            };
            var poco = context.Students.Include(it => it.Enrollments)
                .First(it => it.ID == dto.ID);

            dto.BuildAdapter()
                .EntityFromContext(context)
                .AdaptTo(poco);

            var first = poco.Enrollments.First();
            first.CourseID.ShouldBe(3141);
            first.Grade.ShouldBe(Grade.F);
        }
        
        [TestMethod]
        public async Task TestFindObjectUsingProjectToTypeList()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase($"School-{Guid.NewGuid()}")
                .Options;
            await using var context = new SchoolContext(options);
            DbInitializer.Initialize(context);
            
            var query = context.Students.Where(s => s.ID == 1);

            var list = await query.BuildAdapter().ProjectToType<StudentDto>()
                .OrderBy(s => s.ID)
                .ToListAsync();
            var first = list[0];
            first.ID.ShouldBe(1);
            first.FirstMidName.ShouldBe("Carson");
            first.LastName.ShouldBe("Alexander");
        }

        [TestMethod]
        public async Task TestFindSingleObjectUsingProjectToType()
        {
            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseInMemoryDatabase($"School-{Guid.NewGuid()}")
                .Options;
            await using var context = new SchoolContext(options);
            DbInitializer.Initialize(context);
            
            var query = context.Students.Where(s => s.ID == 1);

            var first = await query.BuildAdapter().ProjectToType<StudentDto>().SingleAsync();
            first.ID.ShouldBe(1);
            first.FirstMidName.ShouldBe("Carson");
            first.LastName.ShouldBe("Alexander");
        }
    }
    public class StudentDto
    {
        public int ID { get; set; }
        public string FirstMidName { get; set; }
        public string LastName { get; set; }
        public ICollection<EnrollmentItemDto> Enrollments { get; set; }
    }

    public class EnrollmentItemDto
    {
        public int EnrollmentID { get; set; }
        public Grade? Grade { get; set; }
    }
}
