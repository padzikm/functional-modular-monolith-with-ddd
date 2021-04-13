#!/bin/sh

sqlpackage /a:Script /tu:sa /tp:DB_Admin /sf:./bin/Debug/netstandard2.0/CompanyName.MyMeetings.Database.dacpac /op:"./Migrations/$1" /tsn:localhost /tdn:MyMeetings
