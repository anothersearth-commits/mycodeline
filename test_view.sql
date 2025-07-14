-- Test if VW_EOM_EMPLOYEES view exists and is accessible
SELECT COUNT(*) FROM VW_EOM_EMPLOYEES;

-- Test a simple query to see if the view works
SELECT EMPLOYEEID, FIRSTNAME, LASTNAME, EMAIL, ISACTIVE 
FROM VW_EOM_EMPLOYEES 
WHERE ROWNUM <= 5;

-- Check if the view exists in the database
SELECT VIEW_NAME, TEXT 
FROM USER_VIEWS 
WHERE VIEW_NAME = 'VW_EOM_EMPLOYEES'; 