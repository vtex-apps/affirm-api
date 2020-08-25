# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.19] - 2020-08-25

 ### Added
 
- Added logging

## [1.1.18] - 2020-08-25

### Fixed

- Changed transaction_id to reference_id

## [1.1.17] - 2020-06-17

### Fixed

- Proxy header

### Added
 
- Katapult urls to outbound access

## [1.1.16] - 2020-06-17

 ### Added
 
- Get Katapult funding information

## [1.1.15] - 2020-06-10

### Fixed

- Capture bugfix
 
 ### Added
 
- Adding support for Katapult funding

## [1.1.14] - 2020-06-02

### Fixed

- On Cancel, if there is no authorization_id, assume the order was never completed and return successful cancellation

## [1.1.13] - 2020-05-28

### Fixed

- Allow partial capture

## [1.1.12] - 2020-05-17

### Fixed

- On auth, do not save payment info if data is missing

## [1.1.11] - 2020-05-11

### Fixed

- If already captured fake success response

## [1.1.10] - 2020-05-01

### Fixed

- If Auth fails, check status in Affirm

## [1.1.9] - 2020-04-24

## [1.1.8] - 2020-04-24

## [1.1.7] - 2020-04-22

## [1.1.5] - 2020-04-22

## [1.1.4] - 2020-04-22

## [1.1.3] - 2020-04-22

## [1.1.1] - 2020-04-21

### Added

- Added support for Katapult

## [0.1.2] - 2019-08-23

### Fixed

- Always save and retrieve vbase order data from master workspace

## [0.1.1] - 2019-08-23

### Added

- Support for new `orderId` field in PaymentRequest object
- Support for payment cancellations triggered through gateway by frontend 
- Use `vtex.affirm-payment` appSettings `isLive` boolean to determine whether transaction is live or sandbox

## [0.1.0] - 2019-08-06

### Added

- First release