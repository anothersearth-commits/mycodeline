
CREATE TABLE DIGIHR_TEST.AD_EMPLOYEE
(
  EMP_ID                     NUMBER(19)         NOT NULL,
  COMPANY_EMP_ID             VARCHAR2(50 BYTE),
  APP_USER_ID                VARCHAR2(100 BYTE),
  EMP_NAME                   VARCHAR2(100 BYTE) NOT NULL,
  EMP_TYPE                   VARCHAR2(20 BYTE)  NOT NULL,
  DESIGNATION                VARCHAR2(100 BYTE),
  DEPARTMENT                 VARCHAR2(100 BYTE),
  DIVISION                   VARCHAR2(100 BYTE),
  STATUS                     NUMBER(3),
  PHONE                      VARCHAR2(30 BYTE),
  GSM                        VARCHAR2(20 BYTE),
  EMAIL                      VARCHAR2(80 BYTE),
  SUPERVISOR_ID              NUMBER(19),
  CONTRACTOR_COMPANY         VARCHAR2(100 BYTE),
  HSE_PP_NO                  VARCHAR2(20 BYTE),
  HSE_PP_ISSUED_BY           VARCHAR2(100 BYTE),
  ADDRESS                    VARCHAR2(300 BYTE),
  DOB                        DATE,
  DRIVING_LICENSE_NO         VARCHAR2(20 BYTE),
  DRIVING_LICENSE_EXPIRY     DATE,
  CIVIL_ID                   VARCHAR2(50 BYTE),
  CIVIL_ID_EXPIRY            DATE,
  PP_NO                      VARCHAR2(50 BYTE),
  PP_EXPIRY                  DATE,
  DD_PERMIT_EXPIRY           DATE,
  NATIONALITY                VARCHAR2(50 BYTE),
  GENDER                     VARCHAR2(1 BYTE),
  FAX_NO                     VARCHAR2(50 BYTE),
  AD_REMOVE_DATE             DATE,
  IS_EXIST_IN_AD             VARCHAR2(1 BYTE),
  PHOTO                      NCLOB,
  INSERT_DATE                DATE               NOT NULL,
  UPDATE_BY                  NUMBER(19),
  UPDATE_DATE                DATE,
  PHOTO_PATH                 VARCHAR2(1000 BYTE),
  PWD                        VARCHAR2(20 BYTE),
  SALARY                     NUMBER(10),
  IS_DRIVER                  NUMBER(3),
  GSM2                       VARCHAR2(20 BYTE),
  VISA_NO                    VARCHAR2(50 BYTE),
  VISA_EXPIRY                DATE,
  PP_DEPOSITED               NUMBER(3),
  DATE_OF_JOIN               DATE,
  IS_USER                    NUMBER(3),
  DATE_OF_LEAVING            DATE,
  EMP_LOC_TYPE               VARCHAR2(1 BYTE),
  LOC_ID                     NUMBER(10),
  COMPANY_ID                 NUMBER(10),
  IS_FIELD_SUPER             NUMBER(3),
  MOB_LOGIN_ACCESS           NUMBER(3),
  WEB_LOGIN_ACCESS           NUMBER(3),
  TS_LOGIN_ACCESS            NUMBER(3),
  IS_WORK_FROM_HOME          NUMBER(3),
  LABOUR_CARD_NO             VARCHAR2(20 BYTE),
  LABOUR_CARD_EXPIRY         DATE,
  EMIRATES_ID_NO             VARCHAR2(20 BYTE),
  EMIRATES_ID_EXPIRY         DATE,
  FILE_NO                    VARCHAR2(30 BYTE),
  REMARKS                    VARCHAR2(500 BYTE),
  LAST_WORKING_DAY           DATE,
  FATHER_NAME                VARCHAR2(50 BYTE),
  MOTHER_NAME                VARCHAR2(50 BYTE),
  SPOUSE_NAME                VARCHAR2(50 BYTE),
  MARITAL_STATUS             VARCHAR2(2 BYTE),
  NO_OF_CHILDREN             NUMBER(3),
  RELIGION                   VARCHAR2(30 BYTE),
  EMP_RELATIVE               NUMBER(19),
  DATE_OF_RESIGN             DATE,
  BLOCK_MOB_LOGIN            VARCHAR2(1 BYTE),
  MOB_DEVICE_ID              NCLOB,
  EMP_NAME_AR                VARCHAR2(100 BYTE),
  QR_LOGIN                   NUMBER(3),
  CALENDAR_HDR_ID            NUMBER(5),
  DESIGNATION_DESC           VARCHAR2(100 BYTE),
  DEPARTMENT_DESC            VARCHAR2(100 BYTE),
  LM                         VARCHAR2(200 BYTE),
  PAYMENT_MODE               VARCHAR2(10 BYTE),
  BRANCH                     VARCHAR2(20 BYTE),
  REGION                     VARCHAR2(20 BYTE),
  SECTION                    VARCHAR2(20 BYTE),
  DEVICE_USER_ID             NUMBER(19),
  IS_MANAGER                 NUMBER(19),
  IS_ADMIN                   NUMBER(19),
  LEAVE_RIGHT                NUMBER(19),
  CALENDAR_RIGHT             NUMBER(19),
  SHIFT_RIGHT                NUMBER(19),
  REPORT_RIGHT               NUMBER(19),
  NOT_SHOW_IN_REPORT         NUMBER(19),
  EMP_CLASS                  VARCHAR2(10 BYTE),
  ADMINISTRATIVE_DIVISION    NUMBER(19),
  GOVERNORATE_BRANCH_OFFICE  NUMBER(19),
  LOC3                       NUMBER(19),
  LOC4                       NUMBER(19),
  DAILY_REPORT_RIGHT         NUMBER(10),
  LEAVE_REPORT_RIGHT         NUMBER(10),
  ABSENSE_REPORT_RIGHT       NUMBER(10),
  WORK_REPORT_RIGHT          NUMBER(10),
  EMPLOYEE_RIGHT             NUMBER(19),
  IS_ADMIN_HEAD              NUMBER(3),
  IS_BRANCH_HEAD             NUMBER(3),
  IS_DEP_HEAD                NUMBER(3),
  IS_SECTION_HEAD            NUMBER(3),
  IS_HR                      NUMBER(3),
  ALL_USER_ACCESS            NUMBER(3),
  JOB_LEVEL                  VARCHAR2(20 BYTE),
  GOVERNOR                   NUMBER(3),
  DIRECTORATE_GENERAL        NUMBER(3),
  IS_EMAIL                   NUMBER(3),
  ROLE_LEVEL                 VARCHAR2(50 BYTE)
)


SET DEFINE OFF;
Insert into DIGIHR_TEST.AD_EMPLOYEE
   (EMP_ID, COMPANY_EMP_ID, APP_USER_ID, EMP_NAME, EMP_TYPE, STATUS, EMAIL, SUPERVISOR_ID, INSERT_DATE, PWD, DATE_OF_JOIN, IS_USER, EMP_LOC_TYPE, COMPANY_ID, EMP_NAME_AR, CALENDAR_HDR_ID, DEVICE_USER_ID, EMP_CLASS, ADMINISTRATIVE_DIVISION, GOVERNORATE_BRANCH_OFFICE, LOC3, LOC4, JOB_LEVEL, ROLE_LEVEL)
 Values
   (1394, '7426', '7426', 'MAJID OMER  ABDULFATTAH  ALSHIZAWI', '1', 
    1, 
    'MAJID.ALSHIZAWI@bng.gov.om', 1932, 
    TO_DATE('01/30/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'), 'Digihr@2024', TO_DATE('02/16/2014 00:00:00', 'MM/DD/YYYY HH24:MI:SS'), 1, 'O', 1, 'ماجد عمر عبدالفتاح الشيزاوي ', 1, 0, '9', 1, 4, 52, 267, 'NU', 'R1');
COMMIT;




CREATE TABLE DIGIHR_TEST.AD_LOCATION
(
  LOC_ID       NUMBER(10)                       NOT NULL,
  TYPE_ID      NUMBER(10)                       NOT NULL,
  LOC_NAME     VARCHAR2(100 BYTE)               NOT NULL,
  LOC_NAME_AR  VARCHAR2(100 BYTE),
  PARENT_ID    NUMBER(10),
  DISPLAY_SEQ  NUMBER(10),
  UPDATE_BY    VARCHAR2(100 BYTE),
  UPDATE_DATE  DATE
)

LOC3 = 52 
LOC4 = 267 
query AD_LOCATION.LOC_ID IN (52, 267) 

LOC3 is department 
LOC4 is the section 


SET DEFINE OFF;
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ, UPDATE_BY, UPDATE_DATE)
 Values
   (267, 4, 'قسم نظم المعلومات والشبكات', 'قسم نظم المعلومات والشبكات', 52, 
    209, '0', TO_DATE('04/08/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'));
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ, UPDATE_BY, UPDATE_DATE)
 Values
   (52, 3, 'دائرة تقنية المعلومات', 'دائرة تقنية المعلومات', 4, 
    40, '0', TO_DATE('04/08/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'));
COMMIT;



this query create view with all manager of departments 

CREATE OR REPLACE FORCE VIEW DIGIHR_TEST.VW_DEPARTMENTS
(
   DEPARTMENTID,
   DESCRIPTION,
   EMP_ID,
   APP_USER_ID,
   EMP_NAME
)
AS
   SELECT AD_LOCATION.LOC_ID DEPARTMENTID,
          AD_LOCATION.LOC_NAME_AR DESCRIPTION,
          ad_employee.EMP_ID EMP_ID,
          ad_employee.APP_USER_ID APP_USER_ID,
          ad_employee.EMP_NAME EMP_NAME
     FROM ad_employee, AD_LOCATION
    WHERE     AD_LOCATION.LOC_ID = ad_employee.loc3(+)
          AND AD_LOCATION.TYPE_ID = 3
          AND ad_employee.IS_DEP_HEAD(+) = 1;





 

SET DEFINE OFF;
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ)
 Values
   (1, 1, 'محافظة شمال الباطنة', 'محافظة شمال الباطنة', 0, 
    1);
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ, UPDATE_BY, UPDATE_DATE)
 Values
   (4, 2, 'المديرية العامة للشؤون الإدارية والمالية', 'المديرية العامة للشؤون الإدارية والمالية', 1, 
    3, '0', TO_DATE('04/08/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'));
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ, UPDATE_BY, UPDATE_DATE)
 Values
   (267, 4, 'قسم نظم المعلومات والشبكات', 'قسم نظم المعلومات والشبكات', 52, 
    209, '0', TO_DATE('04/08/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'));
Insert into DIGIHR_TEST.AD_LOCATION
   (LOC_ID, TYPE_ID, LOC_NAME, LOC_NAME_AR, PARENT_ID, DISPLAY_SEQ, UPDATE_BY, UPDATE_DATE)
 Values
   (52, 3, 'دائرة تقنية المعلومات', 'دائرة تقنية المعلومات', 4, 
    40, '0', TO_DATE('04/08/2025 00:00:00', 'MM/DD/YYYY HH24:MI:SS'));
COMMIT;

