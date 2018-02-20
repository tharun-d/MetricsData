create database metrics


create table EmployeeDetails
(
EmployeeName varchar(max),
ApplicationName varchar(max),
TaskDescription varchar(max),
TaskClassification varchar(max),
AssignedDate varchar(max),
CompletedDate varchar(max),
EffortHours int
)


Create procedure InsertIntoEmployeeDetails
(
@EmployeeName varchar(max),
@ApplicationName varchar(max),
@TaskDescription varchar(max),
@TaskClassification varchar(max),
@AssignedDate varchar(max),
@CompletedDate varchar(max),
@EffortHours int
)
as
begin
insert into EmployeeDetails values(@EmployeeName,@ApplicationName,@TaskDescription,@TaskClassification,@AssignedDate,@CompletedDate,@EffortHours)
end

create table vacationDates
(
EmployeeName varchar(max),
StartDate varchar(max),
EndDate varchar(max),
NumberOfDates int,
Weekend varchar(max) null
)
drop table vacationDates

alter procedure insertintovacationDates
(
@EmployeeName varchar(max),
@StartDate varchar(max),
@EndDate varchar(max),
@NumberOfDates int,
@Weekend varchar(max)
)
as
begin
insert into vacationDates values(@EmployeeName,@StartDate,@EndDate,@NumberOfDates,@Weekend)
end

create table CatwHours
(
EmployeeName varchar(max),
TotalCatwHours int
)

create procedure insertintoCatwHours
(
@EmployeeName varchar(max),
@TotalCatwHours int
)
as
begin
insert into CatwHours values(@EmployeeName,@TotalCatwHours)
end

create table FillHoursTable
(
EmployeeName varchar(max),
TotalCatwHours int
)

create procedure insertintoFillHoursTable
(
@EmployeeName varchar(max),
@TotalCatwHours int
)
as
begin
insert into FillHoursTable values(@EmployeeName,@TotalCatwHours)
end

select * from EmployeeDetails
delete from EmployeeDetails
select * from vacationDates
delete from vacationDates
select * from CatwHours
delete from CatwHours
select * from FillHoursTable
delete from FillHoursTable

alter procedure GettingMissMathedEmployees as
begin
select ch.EmployeeName,ch.TotalCatwHours as MentionedHours,fh.TotalCatwHours as CaluclatedHours from CatwHours ch inner join FillHoursTable fh
on ch.EmployeeName=fh.EmployeeName
where ch.TotalCatwHours!=fh.TotalCatwHours
end


create procedure DeleteMetrics as
begin
delete from EmployeeDetails
delete from vacationDates
delete from CatwHours
delete from FillHoursTable
end