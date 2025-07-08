using Microsoft.EntityFrameworkCore;
using EOM.Web.Models;

namespace EOM.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        // Create test employees (HR data)
        await CreateEmployeesAsync(context);
        
        // Create test managers hierarchy
        await CreateManagersAsync(context);
        
        // Create award types and criteria
        await CreateAwardTypesAsync(context);
        
        // Create sub-criteria if they don't exist
        await CreateSubCriteriaIfNeededAsync(context);
        
        // Update existing employees with passwords if needed
        await UpdateEmployeePasswordsAsync(context);
        
        // Create test committee members
        await CreateCommitteeMembersAsync(context);
        
        // Create department quotas
        await CreateDepartmentQuotasAsync(context);
        
        // Create award cycles
        await CreateAwardCyclesAsync(context);
    }

    private static async Task CreateEmployeesAsync(ApplicationDbContext context)
    {
        if (!context.Employees.Any())
        {
            var employees = new List<Employee>
            {
                // Admin
                new Employee
                {
                    EmployeeId = 1,
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
                    EmployeeId = 2,
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
                    EmployeeId = 3,
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
                    EmployeeId = 4,
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
                    EmployeeId = 5,
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
                    EmployeeId = 6,
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
                    EmployeeId = 7,
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

    private static async Task CreateManagersAsync(ApplicationDbContext context)
    {
        if (!context.EmployeeManagers.Any())
        {
            var managers = new List<EmployeeManager>
            {
                // Sara manages Mohammed and Aisha (IT Department)
                new EmployeeManager
                {
                    EmployeeId = 5, // Mohammed
                    ManagerId = 2,  // Sara
                    StartDate = DateTime.Now.AddYears(-1)
                },
                new EmployeeManager
                {
                    EmployeeId = 6, // Aisha
                    ManagerId = 2,  // Sara
                    StartDate = DateTime.Now.AddYears(-1)
                },
                // Khalid manages Omar (Sales Department)
                new EmployeeManager
                {
                    EmployeeId = 7, // Omar
                    ManagerId = 3,  // Khalid
                    StartDate = DateTime.Now.AddMonths(-8)
                }
            };

            context.EmployeeManagers.AddRange(managers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task CreateAwardTypesAsync(ApplicationDbContext context)
    {
        if (!context.AwardTypes.Any())
        {
            var employeeOfMonth = new AwardType
            {
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
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "الالتزام والانضباط",
                    WeightPercent = 25.0m
                },
                new Criterion
                {
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "جودة الخدمة والإنتاجية",
                    WeightPercent = 30.0m
                },
                new Criterion
                {
                    AwardTypeId = employeeOfMonth.AwardTypeId,
                    Name = "التعاون والعمل الجماعي",
                    WeightPercent = 25.0m
                },
                new Criterion
                {
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
                    EmployeeId = 4, // Fatima
                    StartDate = DateTime.UtcNow,
                    IsActive = true
                },
                new CommitteeMember
                {
                    EmployeeId = 5, // Mohammed 
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
            subCriteria.AddRange(new[]
            {
                new SubCriteria
                {
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.1",
                    Name = "الالتزام بالحضور والانصراف",
                    MaxScore = 8,
                    GradingScale = """[{"score": 8, "description": "حضور منتظم دون غيابات/ تأخيرات"}, {"range": "4-7", "description": "تأخيرات طفيفة/عرضية لا تتكرر"}, {"range": "0-3", "description": "تأخيرات متكررة/غياب غير مبرر"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.2",
                    Name = "الانضباط في المواعيد",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "حضور في الوقت المحدد لجميع الالتزامات"}, {"range": "5-2", "description": "التزام جيد مع تأخيرات بسيطة"}, {"range": "1-0", "description": "تأخر متكرر/تجاهل المواعيد"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion1.CriterionId,
                    SubCriteriaCode = "1.3",
                    Name = "التمثيل المؤسسي والسلوك المهني",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "تمثيل مؤسسي وسلوك مهني احترافي"}, {"range": "4-2", "description": "تمثيل مؤسسي وسلوك مهني متوسط"}, {"range": "1-0", "description": "تمثيل مؤسسي وسلوك مهني ضعيف"}]"""
                },
                new SubCriteria
                {
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
                    CriterionId = criterion2.CriterionId,
                    SubCriteriaCode = "2.1",
                    Name = "دقة تنفيذ المهام ومطابقتها للمتطلبات",
                    MaxScore = 10,
                    GradingScale = """[{"score": 10, "description": "مهمة بجودة وكفاءة عالية"}, {"range": "9-4", "description": "مهمة بجودة وكفاءة متوسطة"}, {"range": "3-0", "description": "مهمة بجودة منخفضة وكفاءة تتطلب إلى تحسين"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion2.CriterionId,
                    SubCriteriaCode = "2.2",
                    Name = "سرعة الإنجاز",
                    MaxScore = 10,
                    GradingScale = """[{"score": 10, "description": "تنفيذ المهام قبل/ضمن الوقت المحدد دائما"}, {"range": "9-4", "description": "تأخير محدود/طارئ غير متكرر"}, {"range": "3-0", "description": "تأخير متكرر/بطء واضح في تقديم الخدمة"}]"""
                },
                new SubCriteria
                {
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
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.1",
                    Name = "الدعم والمبادرة",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "يبادر ويقدم الدعم لفريق العمل"}, {"range": "5-3", "description": "يدعم الفريق عند الحاجة/بناء على تكليف"}, {"range": "0-2", "description": "يتأخر في تقديم المساعدة/يمتنع المبادرة"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.2",
                    Name = "العمل بروح الفريق",
                    MaxScore = 8,
                    GradingScale = """[{"score": 8, "description": "المشاركة الفاعلة والمؤثرة في فرق العمل"}, {"range": "7-3", "description": "التعاون الجيد مع الفريق"}, {"range": "0-2", "description": "عدم التفاعل/ محدودية التفاعل في العمل الجماعي"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion3.CriterionId,
                    SubCriteriaCode = "3.3",
                    Name = "التواصل الإيجابي",
                    MaxScore = 6,
                    GradingScale = """[{"score": 6, "description": "تواصل فعال وأسلوب بناء في التعامل"}, {"range": "5-3", "description": "تواصل مقبول ولكن يفتقر الاستمرارية/التأثير"}, {"range": "0-2", "description": "تواصل ضعيف/يحمل طابع سلبي"}]"""
                },
                new SubCriteria
                {
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
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.1",
                    Name = "تحمل المسؤولية",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتحمل المسؤولية بدرجة عالية"}, {"range": "4-2", "description": "يتحمل المسؤولية عند المتابعة"}, {"range": "1-0", "description": "مستوى متدنٍ في تحمل المسؤولية"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.2",
                    Name = "التعامل المهني مع المواقف الصعبة",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتعامل باحترافية في كل المواقف"}, {"range": "4-2", "description": "يتعامل بإيجابية مع ضغوط العمل"}, {"range": "1-0", "description": "ردود فعل غير مهنية"}]"""
                },
                new SubCriteria
                {
                    CriterionId = criterion4.CriterionId,
                    SubCriteriaCode = "4.3",
                    Name = "القدرة على اتخاذ القرار",
                    MaxScore = 5,
                    GradingScale = """[{"score": 5, "description": "يتخذ القرار المناسب في حدود مسؤولياته"}, {"range": "4-2", "description": "يتخذ قرارات مدروسة بتوجيه"}, {"range": "1-0", "description": "غير قادر على اتخاذ القرارات"}]"""
                },
                new SubCriteria
                {
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
                        Quota = 2,
                        MaxNominations = 2
                    },
                    new DepartmentQuota
                    {
                        DepartmentId = 20, // Sales Department
                        AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                        Quota = 1,
                        MaxNominations = 1
                    },
                    new DepartmentQuota
                    {
                        DepartmentId = 30, // Finance Department
                        AwardTypeId = employeeOfMonthAwardType.AwardTypeId,
                        Quota = 1,
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