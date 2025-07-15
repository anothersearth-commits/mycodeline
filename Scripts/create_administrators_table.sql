-- Create Oracle sequence for Administrators table
CREATE SEQUENCE SEQ_ADMINISTRATOR START WITH 1 INCREMENT BY 1;

-- Create Administrators table
CREATE TABLE Administrators (
    AdministratorId NUMBER(10) DEFAULT SEQ_ADMINISTRATOR.NEXTVAL NOT NULL,
    EmployeeId NUMBER(10) NOT NULL,
    IsActive NUMBER(1) DEFAULT 1 NOT NULL,
    CONSTRAINT PK_Administrators PRIMARY KEY (AdministratorId)
);

-- Create index for performance
CREATE INDEX IX_Administrator_Employee ON Administrators (EmployeeId);

-- Insert sample administrators (optional)
-- You can run these after creating the table if you want test data:
-- INSERT INTO Administrators (EmployeeId, IsActive) VALUES (1, 1);  -- Admin employee
-- INSERT INTO Administrators (EmployeeId, IsActive) VALUES (2, 1);  -- Another admin

-- Verify table creation
SELECT COUNT(*) as "Table_Created" FROM user_tables WHERE table_name = 'ADMINISTRATORS'