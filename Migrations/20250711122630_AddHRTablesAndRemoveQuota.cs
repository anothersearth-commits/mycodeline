using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EOM.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHRTablesAndRemoveQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Check if Quota column exists before dropping it
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
                              WHERE TABLE_SCHEMA = 'EOM' 
                              AND TABLE_NAME = 'DepartmentQuotas' 
                              AND COLUMN_NAME = 'Quota');
                SET @sqlstmt := IF(@exist > 0, 'ALTER TABLE DepartmentQuotas DROP COLUMN Quota', 'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "EvaluationScores",
                type: "int",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint unsigned");

            // Check if EvaluationScoreId column exists before adding it
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.COLUMNS 
                              WHERE TABLE_SCHEMA = 'EOM' 
                              AND TABLE_NAME = 'EvaluationScores' 
                              AND COLUMN_NAME = 'EvaluationScoreId');
                SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE EvaluationScores ADD EvaluationScoreId int NOT NULL DEFAULT 0', 'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Create Departments table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `Departments` (
                    `DepartmentId` int NOT NULL AUTO_INCREMENT,
                    `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
                    `Description` varchar(200) CHARACTER SET utf8mb4 NULL,
                    `IsActive` tinyint(1) NOT NULL,
                    CONSTRAINT `PK_Departments` PRIMARY KEY (`DepartmentId`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create VW_EOM_DEPARTMENTS table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `VW_EOM_DEPARTMENTS` (
                    `DEPARTMENTID` bigint NOT NULL AUTO_INCREMENT,
                    `NAME` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `DESCRIPTION` longtext CHARACTER SET utf8mb4 NULL,
                    `ISACTIVE` tinyint(1) NOT NULL,
                    CONSTRAINT `PK_VW_EOM_DEPARTMENTS` PRIMARY KEY (`DEPARTMENTID`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create VW_EOM_EMPLOYEES table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `VW_EOM_EMPLOYEES` (
                    `EMPLOYEEID` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `FIRSTNAME` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `LASTNAME` longtext CHARACTER SET utf8mb4 NOT NULL,
                    `EMAIL` longtext CHARACTER SET utf8mb4 NULL,
                    `DEPARTMENTID` bigint NOT NULL,
                    `JOBTITLE` longtext CHARACTER SET utf8mb4 NULL,
                    `HIREDATE` datetime(6) NULL,
                    `ACTIVEDIRECTORYID` longtext CHARACTER SET utf8mb4 NULL,
                    `PASSWORD` longtext CHARACTER SET utf8mb4 NULL,
                    `ISACTIVE` tinyint(1) NOT NULL,
                    `PHONENUMBER` longtext CHARACTER SET utf8mb4 NULL,
                    `MANAGERID` longtext CHARACTER SET utf8mb4 NULL,
                    `MANAGERNAME` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_VW_EOM_EMPLOYEES` PRIMARY KEY (`EMPLOYEEID`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create VW_EOM_MANAGERS table if it doesn't exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS `VW_EOM_MANAGERS` (
                    `MANAGERID` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
                    `MANAGERNAME` longtext CHARACTER SET utf8mb4 NULL,
                    `MANAGERNAME_AR` longtext CHARACTER SET utf8mb4 NULL,
                    `EMAIL` longtext CHARACTER SET utf8mb4 NULL,
                    `DEPARTMENTID` bigint NOT NULL,
                    `DEPARTMENTNAME` longtext CHARACTER SET utf8mb4 NULL,
                    `JOBTITLE` longtext CHARACTER SET utf8mb4 NULL,
                    `PHONE` longtext CHARACTER SET utf8mb4 NULL,
                    `ACTIVEDIRECTORYID` longtext CHARACTER SET utf8mb4 NULL,
                    CONSTRAINT `PK_VW_EOM_MANAGERS` PRIMARY KEY (`MANAGERID`)
                ) CHARACTER SET=utf8mb4;
            ");

            // Create index if it doesn't exist
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.statistics 
                              WHERE table_schema = 'EOM' 
                              AND table_name = 'Employees' 
                              AND index_name = 'IX_Employees_DepartmentId');
                SET @sqlstmt := IF(@exist = 0, 'CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId)', 'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Add foreign key constraints if they don't exist
            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.table_constraints 
                              WHERE constraint_schema = 'EOM' 
                              AND table_name = 'DepartmentQuotas' 
                              AND constraint_name = 'FK_DepartmentQuotas_Departments_DepartmentId');
                SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE DepartmentQuotas ADD CONSTRAINT FK_DepartmentQuotas_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE CASCADE', 'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @exist := (SELECT COUNT(*) FROM information_schema.table_constraints 
                              WHERE constraint_schema = 'EOM' 
                              AND table_name = 'Employees' 
                              AND constraint_name = 'FK_Employees_Departments_DepartmentId');
                SET @sqlstmt := IF(@exist = 0, 'ALTER TABLE Employees ADD CONSTRAINT FK_Employees_Departments_DepartmentId FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE CASCADE', 'SELECT 1');
                PREPARE stmt FROM @sqlstmt;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepartmentQuotas_Departments_DepartmentId",
                table: "DepartmentQuotas");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "VW_EOM_DEPARTMENTS");

            migrationBuilder.DropTable(
                name: "VW_EOM_EMPLOYEES");

            migrationBuilder.DropTable(
                name: "VW_EOM_MANAGERS");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EvaluationScoreId",
                table: "EvaluationScores");

            migrationBuilder.AlterColumn<byte>(
                name: "Score",
                table: "EvaluationScores",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quota",
                table: "DepartmentQuotas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
