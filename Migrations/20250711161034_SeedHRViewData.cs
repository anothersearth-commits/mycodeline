using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class SeedHRViewData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing tables if they exist and create views
            migrationBuilder.Sql("DROP TABLE IF EXISTS VW_EOM_DEPARTMENTS");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_DEPARTMENTS");
            
            // Create VW_EOM_DEPARTMENTS view
            migrationBuilder.Sql(@"
                CREATE VIEW VW_EOM_DEPARTMENTS AS
                SELECT 
                    10 as DEPARTMENTID,
                    'قسم تقنية المعلومات' as NAME,
                    'إدارة الأنظمة والبرمجيات وقواعد البيانات' as DESCRIPTION,
                    1 as ISACTIVE
                UNION ALL
                SELECT 
                    20 as DEPARTMENTID,
                    'قسم المبيعات' as NAME,
                    'إدارة المبيعات وخدمة العملاء' as DESCRIPTION,
                    1 as ISACTIVE
                UNION ALL
                SELECT 
                    30 as DEPARTMENTID,
                    'قسم المحاسبة' as NAME,
                    'إدارة الحسابات المالية والمراجعة' as DESCRIPTION,
                    1 as ISACTIVE
                UNION ALL
                SELECT 
                    40 as DEPARTMENTID,
                    'قسم الموارد البشرية' as NAME,
                    'إدارة شؤون الموظفين والتوظيف' as DESCRIPTION,
                    1 as ISACTIVE");

            // Create VW_EOM_EMPLOYEES view
            migrationBuilder.Sql("DROP TABLE IF EXISTS VW_EOM_EMPLOYEES");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_EMPLOYEES");
            
            migrationBuilder.Sql(@"
                CREATE VIEW VW_EOM_EMPLOYEES AS
                SELECT 
                    '1' as EMPLOYEEID,
                    'أحمد' as FIRSTNAME,
                    'المدير' as LASTNAME,
                    'admin@company.com' as EMAIL,
                    '123-456-7890' as PHONENUMBER,
                    10 as DEPARTMENTID,
                    'مدير النظام' as JOBTITLE,
                    '2020-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'admin@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '2' as EMPLOYEEID,
                    'سارة' as FIRSTNAME,
                    'محمد' as LASTNAME,
                    'sara@company.com' as EMAIL,
                    '123-456-7891' as PHONENUMBER,
                    10 as DEPARTMENTID,
                    'مدير قسم التقنية' as JOBTITLE,
                    '2022-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'sara@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '3' as EMPLOYEEID,
                    'خالد' as FIRSTNAME,
                    'أحمد' as LASTNAME,
                    'khalid@company.com' as EMAIL,
                    '123-456-7892' as PHONENUMBER,
                    20 as DEPARTMENTID,
                    'مدير قسم المبيعات' as JOBTITLE,
                    '2021-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'khalid@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '4' as EMPLOYEEID,
                    'فاطمة' as FIRSTNAME,
                    'علي' as LASTNAME,
                    'fatima@company.com' as EMAIL,
                    '123-456-7893' as PHONENUMBER,
                    30 as DEPARTMENTID,
                    'محاسب أول' as JOBTITLE,
                    '2023-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'fatima@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '5' as EMPLOYEEID,
                    'محمد' as FIRSTNAME,
                    'حسن' as LASTNAME,
                    'mohammed@company.com' as EMAIL,
                    '123-456-7894' as PHONENUMBER,
                    10 as DEPARTMENTID,
                    'مطور برمجيات' as JOBTITLE,
                    '2024-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'mohammed@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '6' as EMPLOYEEID,
                    'عائشة' as FIRSTNAME,
                    'يوسف' as LASTNAME,
                    'aisha@company.com' as EMAIL,
                    '123-456-7895' as PHONENUMBER,
                    10 as DEPARTMENTID,
                    'مطور واجهات' as JOBTITLE,
                    '2024-01-01' as HIREDATE,
                    1 as ISACTIVE,
                    'aisha@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD
                UNION ALL
                SELECT 
                    '7' as EMPLOYEEID,
                    'عمر' as FIRSTNAME,
                    'إبراهيم' as LASTNAME,
                    'omar@company.com' as EMAIL,
                    '123-456-7896' as PHONENUMBER,
                    20 as DEPARTMENTID,
                    'مندوب مبيعات' as JOBTITLE,
                    '2024-05-01' as HIREDATE,
                    1 as ISACTIVE,
                    'omar@company.com' as ACTIVEDIRECTORYID,
                    '123456' as PASSWORD");

            // Create VW_EOM_MANAGERS view
            migrationBuilder.Sql("DROP TABLE IF EXISTS VW_EOM_MANAGERS");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_MANAGERS");
            
            migrationBuilder.Sql(@"
                CREATE VIEW VW_EOM_MANAGERS AS
                SELECT 
                    '2' as MANAGERID,
                    'سارة محمد' as MANAGERNAME,
                    10 as DEPARTMENTID,
                    'قسم تقنية المعلومات' as DEPARTMENTNAME,
                    1 as ISACTIVE
                UNION ALL
                SELECT 
                    '3' as MANAGERID,
                    'خالد أحمد' as MANAGERNAME,
                    20 as DEPARTMENTID,
                    'قسم المبيعات' as DEPARTMENTNAME,
                    1 as ISACTIVE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the HR views
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_MANAGERS");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_EMPLOYEES");
            migrationBuilder.Sql("DROP VIEW IF EXISTS VW_EOM_DEPARTMENTS");
        }
    }
}
