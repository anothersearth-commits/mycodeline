using Microsoft.EntityFrameworkCore;
using EOM.Web.Models;

namespace EOM.Web.Data;

public static class SeedData
{
    // Helper method to get Oracle sequence next value
    private static async Task<int> GetSequenceNextValue(ApplicationDbContext context, string sequenceName)
    {
        using var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        // Seed the three tables: AwardTypes, Criteria, SubCriteria, and CommitteeMembers
        await CreateAwardTypesAsync(context);
        await CreateCommitteeMembersAsync(context);
    }

    // Employee data now comes from VW_EOM_EMPLOYEES HR view, no seeding needed
    private static async Task CreateEmployeeRecordsAsync(ApplicationDbContext context)
    {
        // If there are already employees in the view/table, skip.
        if (await context.Employees.AnyAsync())
            return;

        var employees = new List<Employee>
        {
            // Admin (will get "EOM-Admin" role)
            new Employee
            {
                EmployeeId = 1,  // Keep explicit IDs for Employee since it's a view and IDs must match HR system
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@company.com",
                DepartmentId = 10,
                JobTitle = "System Administrator",
                HireDate = DateTime.UtcNow.AddYears(-5),
                Password = "123456",
                IsActive = 1,
                IsManager = 0
            },
            // Manager Sara (id 2) – will get Manager role via IsManager = 1
            new Employee
            {
                EmployeeId = 2,
                FirstName = "Sara",
                LastName = "Mohammed",
                Email = "sara@company.com",
                DepartmentId = 10,
                JobTitle = "IT Manager",
                HireDate = DateTime.UtcNow.AddYears(-3),
                Password = "123456",
                IsActive = 1,
                IsManager = 1
            },
            // Committee member Fatima (id 4)
            new Employee
            {
                EmployeeId = 4,
                FirstName = "Fatima",
                LastName = "Ali",
                Email = "fatima@company.com",
                DepartmentId = 30,
                JobTitle = "Senior Accountant",
                HireDate = DateTime.UtcNow.AddYears(-2),
                Password = "123456",
                IsActive = 1,
                IsManager = 0
            },
            // Committee member Mohammed (id 5)
            new Employee
            {
                EmployeeId = 5,
                FirstName = "Mohammed",
                LastName = "Hassan",
                Email = "mohammed@company.com",
                DepartmentId = 10,
                JobTitle = "Software Developer",
                HireDate = DateTime.UtcNow.AddYears(-1),
                Password = "123456",
                IsActive = 1,
                IsManager = 0
            },
            // Dual-role user: Manager + Committee member (id 6)
            new Employee
            {
                EmployeeId = 6,
                FirstName = "Ahmed",
                LastName = "Salem",
                Email = "ahmed@company.com",
                DepartmentId = 20,
                JobTitle = "Department Manager",
                HireDate = DateTime.UtcNow.AddYears(-4),
                Password = "123456",
                IsActive = 1,
                IsManager = 1  // This makes him a Manager
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }

    /* DISABLED - Old complex HR data seeding
    private static async Task CreateEmployeesAsync(ApplicationDbContext context)
    {
        if (!context.VwEomEmployees.Any())
        {
            var employees = new List<VwEomEmployees>
            {
                // Admin
                new VwEomEmployees
                {
                    EmployeeId = "1",
                    FirstName = "أحمد",
                    LastName = "المدير",
                    Email = "admin@company.com",
                    DepartmentId = 1,
                    JobTitle = "مدير النظام",
                    HireDate = DateTime.Now.AddYears(-5),
                    ActiveDirectoryId = "admin@company.com",
                    Password = "123456"
                },
                // Managers
                new Employee
                {
                    EmployeeId = "2",
                    FirstName = "سارة",
                    LastName = "محمد",
                    Email = "sara@company.com",
                    DepartmentId = 10,
                    JobTitle = "مدير قسم التقنية",
                    HireDate = DateTime.Now.AddYears(-3),
                    ActiveDirectoryId = "sara@company.com",
                    Password = "123456"
                },
                new Employee
                {
                    EmployeeId = "3",
                    FirstName = "خالد",
                    LastName = "أحمد",
                    Email = "khalid@company.com",
                    DepartmentId = 20,
                    JobTitle = "مدير قسم المبيعات",
                    HireDate = DateTime.Now.AddYears(-4),
                    ActiveDirectoryId = "khalid@company.com",
                    Password = "123456"
                },
                // Committee members
                new Employee
                {
                    EmployeeId = "4",
                    FirstName = "فاطمة",
                    LastName = "علي",
                    Email = "fatima@company.com",
                    DepartmentId = 30,
                    JobTitle = "محاسب أول",
                    HireDate = DateTime.Now.AddYears(-2),
                    ActiveDirectoryId = "fatima@company.com",
                    Password = "123456"
                },
                new Employee
                {
                    EmployeeId = "5",
                    FirstName = "محمد",
                    LastName = "حسن",
                    Email = "mohammed@company.com",
                    DepartmentId = 10,
                    JobTitle = "مطور برمجيات",
                    HireDate = DateTime.Now.AddYears(-1),
                    ActiveDirectoryId = "mohammed@company.com",
                    Password = "123456"
                },
                // Regular employees
                new Employee
                {
                    EmployeeId = "6",
                    FirstName = "عائشة",
                    LastName = "يوسف",
                    Email = "aisha@company.com",
                    DepartmentId = 10,
                    JobTitle = "مطور واجهات",
                    HireDate = DateTime.Now.AddYears(-1),
                    ActiveDirectoryId = "aisha@company.com",
                    Password = "123456"
                },
                new Employee
                {
                    EmployeeId = "7",
                    FirstName = "عمر",
                    LastName = "إبراهيم",
                    Email = "omar@company.com",
                    DepartmentId = 20,
                    JobTitle = "مندوب مبيعات",
                    HireDate = DateTime.Now.AddMonths(-8),
                    ActiveDirectoryId = "omar@company.com",
                    Password = "123456"
                }
            };

            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
        }
    }
    */ // End of disabled HR seed methods

    /* DISABLED - HR data should come from actual HR system
    private static async Task CreateManagersAsync(ApplicationDbContext context)
    {
        if (!context.EmployeeManagers.Any())
        {
            var managers = new List<EmployeeManager>
            {
                // Sara manages Mohammed and Aisha (IT Department)
                new EmployeeManager
                {
                    EmployeeId = "5", // Mohammed
                    ManagerId = 2,  // Sara
                    StartDate = DateTime.Now.AddYears(-1)
                },
                new EmployeeManager
                {
                    EmployeeId = "6", // Aisha
                    ManagerId = 2,  // Sara
                    StartDate = DateTime.Now.AddYears(-1)
                },
                // Khalid manages Omar (Sales Department)
                new EmployeeManager
                {
                    EmployeeId = "7", // Omar
                    ManagerId = 3,  // Khalid
                    StartDate = DateTime.Now.AddMonths(-8)
                }
            };

            context.EmployeeManagers.AddRange(managers);
            await context.SaveChangesAsync();
        }
    }
    */ // End of disabled CreateManagersAsync

    private static async Task CreateAwardTypesAsync(ApplicationDbContext context)
    {
        if (!context.AwardTypes.Any())
        {
            // Get next sequence value for Oracle
            var nextId = await GetSequenceNextValue(context, "SEQ_AWARDTYPE");
            
            var employeeOfMonth = new AwardType
            {
                AwardTypeId = nextId,
                Name = "موظف الشهر",
                Description = "Employee of the Month Award",
                IsActive = true
            };

            context.AwardTypes.Add(employeeOfMonth);
            await context.SaveChangesAsync();

            // Create criteria for Employee of the Month
            var criteria = new List<Criterion>
            {
                new Criterion
                {
                    CriterionId = await GetSequenceNextValue(context, "SEQ_CRITERION"),
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "الالتزام والانضباط",
                    WeightPercent = 25.0m
                },
                new Criterion
                {
                    CriterionId = await GetSequenceNextValue(context, "SEQ_CRITERION"),
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "جودة الخدمة والإنتاجية",
                    WeightPercent = 30.0m
                },
                new Criterion
                {
                    CriterionId = await GetSequenceNextValue(context, "SEQ_CRITERION"),
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "التعاون والعمل الجماعي",
                    WeightPercent = 25.0m
                },
                new Criterion
                {
                    CriterionId = await GetSequenceNextValue(context, "SEQ_CRITERION"),
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "الأداء العام",
                    WeightPercent = 20.0m
                }
            };

            context.Criteria.AddRange(criteria);
            await context.SaveChangesAsync();
            
            // Create sub-criteria based on form.md
            await CreateSubCriteriaAsync(context, criteria);
        }
    }

    private static async Task CreateCommitteeMembersAsync(ApplicationDbContext context)
    {
        if (!context.CommitteeMembers.Any())
        {
            var committeeMembers = new List<CommitteeMember>
            {
                new CommitteeMember
                {
                    // Don't set Id - let Oracle sequence/trigger handle it
                    EmployeeId = 1, // Admin acts as committee member too for testing
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                },
                new CommitteeMember
                {
                    // Don't set Id - let Oracle sequence/trigger handle it
                    EmployeeId = 4, // Fatima
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                },
                new CommitteeMember
                {
                    // Don't set Id - let Oracle sequence/trigger handle it
                    EmployeeId = 5, // Mohammed
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                },
                new CommitteeMember
                {
                    // Don't set Id - let Oracle sequence/trigger handle it
                    EmployeeId = 6, // Ahmed - dual role (Manager + Committee)
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                }
            };

            context.CommitteeMembers.AddRange(committeeMembers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task CreateSubCriteriaAsync(ApplicationDbContext context, List<Criterion> criteria)
    {
        if (!context.SubCriteria.Any())
        {
            var subCriteria = new List<SubCriteria>();

            // الالتزام والانضباط (25%)
            var criterion1 = criteria.First(c => c.Name == "الالتزام والانضباط");
            
            // Generate sequence IDs for all 16 SubCriteria at once
            var subCriteriaIds = new List<int>();
            for (int i = 0; i < 16; i++)
            {
                subCriteriaIds.Add(await GetSequenceNextValue(context, "SEQ_SUBCRITERIA"));
            }
            int idIndex = 0;
            
            subCriteria.AddRange(new[]
            {
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.1",
                    Name = "الالتزام بالحضور والانصراف",
                    MaxScore = 8,
                    GradingScale = """[{"score": 8, "description": "حضور منتظم دون غيابات/ تأخيرات"}, {"range": "4-7", "description": "تأخيرات طفيفة/عرضية لا تتكرر"}, {"range": "0-3", "description": "تأخيرات متكررة/غياب غير مبرر"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.2",
                    Name = "الانضباط في المواعيد",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "حضور في الوقت المحدد لجميع الالتزامات"}, {"range": "5-2", "description": "التزام جيد مع تأخيرات بسيطة"}, {"range": "1-0", "description": "تأخر متكرر/تجاهل المواعيد"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.3",
                    Name = "التمثيل المؤسسي والسلوك المهني",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "تمثيل مؤسسي وسلوك مهني احترافي"}, {"range": "4-2", "description": "تمثيل مؤسسي وسلوك مهني متوسط"}, {"range": "1-0", "description": "تمثيل مؤسسي وسلوك مهني ضعيف"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.4",
                    Name = "احترام السياسات وقوانين العمل",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "يلتزم بجميع السياسات دون أي ملاحظات"}, {"range": "5-3", "description": "يوجد تنبيه واحد دون إنذارات"}, {"range": "2-0", "description": "وجود أكثر من تنبيه"}]"""
                }
            });

            // جودة الخدمة والإنتاجية "رضا المستفيد" (30%)
            var criterion2 = criteria.First(c => c.Name == "جودة الخدمة والإنتاجية");
            subCriteria.AddRange(new[]
            {
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion2.CriterionId,
                    SubCriteriaCode = "2.1",
                    Name = "دقة تنفيذ المهام ومطابقتها للمتطلبات",
                    MaxScore = 10,
                    GradingScale = """[{"score": 10, "description": "مهمة بجودة وكفاءة عالية"}, {"range": "9-4", "description": "مهمة بجودة وكفاءة متوسطة"}, {"range": "3-0", "description": "مهمة بجودة منخفضة وكفاءة تتطلب إلى تحسين"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion2.CriterionId,
                    SubCriteriaCode = "2.2",
                    Name = "سرعة الإنجاز",
                    MaxScore = 10,
                    GradingScale = """[{"score": 10, "description": "تنفيذ المهام قبل/ضمن الوقت المحدد دائما"}, {"range": "9-4", "description": "تأخير محدود/طارئ غير متكرر"}, {"range": "3-0", "description": "تأخير متكرر/بطء واضح في تقديم الخدمة"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion2.CriterionId,
                    SubCriteriaCode = "2.3",
                    Name = "المساهمة في تحقيق أهداف الفريق/المؤسسة",
                    MaxScore = 10,
                    GradingScale = """[{"score": 10, "description": "مهام/ خدمات تساهم بشكل مباشر ولها أثر في تحقيق أهداف المؤسسة"}, {"range": "9-4", "description": "مساهمة جيدة لكنها غير رئيسية ومباشرة"}, {"range": "3-0", "description": "مشاركة محدودة/غير مرتبطة"}]"""
                }
            });

            // التعاون والعمل الجماعي (25%)
            var criterion3 = criteria.First(c => c.Name == "التعاون والعمل الجماعي");
            subCriteria.AddRange(new[]
            {
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.1",
                    Name = "الدعم والمبادرة",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "يبادر ويقدم الدعم لفريق العمل"}, {"range": "5-3", "description": "يدعم الفريق عند الحاجة/بناء على تكليف"}, {"range": "0-2", "description": "يتأخر في تقديم المساعدة/يمتنع المبادرة"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.2",
                    Name = "العمل بروح الفريق",
                    MaxScore = 8,
                    GradingScale = """[{"score": 8, "description": "المشاركة الفاعلة والمؤثرة في فرق العمل"}, {"range": "7-3", "description": "التعاون الجيد مع الفريق"}, {"range": "0-2", "description": "عدم التفاعل/ محدودية التفاعل في العمل الجماعي"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.3",
                    Name = "التواصل الإيجابي",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "تواصل فعال وأسلوب بناء في التعامل"}, {"range": "5-3", "description": "تواصل مقبول ولكن يفتقر الاستمرارية/التأثير"}, {"range": "0-2", "description": "تواصل ضعيف/يحمل طابع سلبي"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.4",
                    Name = "تبادل الخبرات ونقل المعرفة",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يشارك المعرفة ويوجه الزملاء بشكل مستمر"}, {"range": "4-3", "description": "يشارك عند الطلب/في مواقف محددة"}, {"range": "0-2", "description": "يمتنع عن نقل الخبرات/يفتقر المشاركة"}]"""
                }
            });

            // الأداء العام (20%)
            var criterion4 = criteria.First(c => c.Name == "الأداء العام");
            subCriteria.AddRange(new[]
            {
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.1",
                    Name = "تحمل المسؤولية",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتحمل المسؤولية بدرجة عالية"}, {"range": "4-2", "description": "يتحمل المسؤولية عند المتابعة"}, {"range": "1-0", "description": "مستوى متدنٍ في تحمل المسؤولية"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.2",
                    Name = "التعامل المهني مع المواقف الصعبة",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتعامل باحترافية في كل المواقف"}, {"range": "4-2", "description": "يتعامل بإيجابية مع ضغوط العمل"}, {"range": "1-0", "description": "ردود فعل غير مهنية"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.3",
                    Name = "القدرة على اتخاذ القرار",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتخذ القرار المناسب في حدود مسؤولياته"}, {"range": "4-2", "description": "يتخذ قرارات مدروسة بتوجيه"}, {"range": "1-0", "description": "غير قادر على اتخاذ القرارات"}]"""
                },
                new SubCriteria
                {
                    SubCriteriaId = subCriteriaIds[idIndex++],
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.4",
                    Name = "رأي المسؤول المباشر",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "أداء مهني ممتاز"}, {"range": "4-2", "description": "أداء مهني جيد"}, {"range": "1-0", "description": "أداء مهني يحتاج إلى تحسين"}]"""
                }
            });

            context.SubCriteria.AddRange(subCriteria);
            await context.SaveChangesAsync();
        }
    }

    private static async Task CreateSubCriteriaIfNeededAsync(ApplicationDbContext context)
    {
        if (!context.SubCriteria.Any())
        {
            var criteria = await context.Criteria.ToListAsync();
            if (criteria.Any())
            {
                await CreateSubCriteriaAsync(context, criteria);
            }
        }
    }

    /* DISABLED - Uses old Employee table
    private static async Task UpdateEmployeePasswordsAsync(ApplicationDbContext context)
    {
        // Update existing employees without passwords
        var employeesWithoutPasswords = await context.Employees
            .Where(e => e.Password == null || e.Password == "")
            .ToListAsync();

        if (employeesWithoutPasswords.Any())
        {
            foreach (var employee in employeesWithoutPasswords)
            {
                employee.Password = "123456";
            }
            await context.SaveChangesAsync();
        }
    }
    */ // End of disabled UpdateEmployeePasswordsAsync

    private static async Task CreateDepartmentQuotasAsync(ApplicationDbContext context)
    {
        if (!context.DepartmentQuotas.Any())
        {
            var employeeOfMonthAwardType = await context.AwardTypes
                .FirstOrDefaultAsync(at => at.Name == "موظف الشهر");

            if (employeeOfMonthAwardType != null)
            {
                var departmentQuotas = new List<DepartmentQuota>
                {
                    new DepartmentQuota
                    {
                        DepartmentId = 10, // IT Department
                        AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                        MaxNominations = 2
                    },
                    new DepartmentQuota
                    {
                        DepartmentId = 20, // Sales Department
                        AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                        MaxNominations = 1
                    },
                    new DepartmentQuota
                    {
                        DepartmentId = 30, // Finance Department
                        AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                        MaxNominations = 1
                    }
                };

                context.DepartmentQuotas.AddRange(departmentQuotas);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task CreateAwardCyclesAsync(ApplicationDbContext context)
    {
        if (!context.AwardCycles.Any())
        {
            var employeeOfMonthAwardType = await context.AwardTypes
                .FirstOrDefaultAsync(at => at.Name == "موظف الشهر");

            if (employeeOfMonthAwardType != null)
            {
                var currentDate = DateTime.UtcNow;
                var awardCycle = new AwardCycle
                {
                    AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                    Month = currentDate.Month,
                    Year = currentDate.Year,
                    NominationStart = new DateTime(currentDate.Year, currentDate.Month, 1),
                    NominationEnd = new DateTime(currentDate.Year, currentDate.Month, 15),
                    Status = CycleStatus.Pending  // Changed from Nomination to Pending
                };

                context.AwardCycles.Add(awardCycle);
                await context.SaveChangesAsync();
            }
        }
    }
}