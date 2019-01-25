@echo off
REM Assumes you're in a git repository
REM set SENTRY_AUTH_TOKEN=...
set SENTRY_ORG=opencover

for /f "delims=" %%a in ('sentry-cli releases propose-version') do @set VERSION=%%a

echo %VERSION%

REM Create a release
sentry-cli releases new -p opencover %VERSION%

REM Associate commits with the release
sentry-cli releases set-commits --auto %VERSION%

set SENTRY_AUTH_TOKEN=
set SENTRY_ORG=
set VERSION=
