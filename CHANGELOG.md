# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Added Feature to Void the amount for partially cancelled items on order when capturing the final amount on Invoice
- Managaned by feature flag enablePartialCancellation configure in Vtex Settings of Affirm Payment App
- Added idempotency key for Refund and Void

## [1.3.6] - 2024-03-12

### Fixed
- Handle callback failure

### Changed

- Cypress cy-runner.yml updated

### Updated

- Reusable workflow updated to version 2

## [1.3.5] - 2022-04-15

### Fixed

- Set recommended `service.json` values including maximum `ttl` of 60 minutes

## [1.3.4] - 2022-02-10

### Fixed

- Add minimum `delayToCancel` in CreatePayment response to ensure users have enough time to complete modal before authorization retry is sent
- Log warning when authorization retry results in payment denial

### Added

- SonarCloud analysis on specific branches and PR's

## [1.3.3] - 2022-01-05

### Added

- Added further splunk logs

## [1.3.2] - 2021-12-10

### Fixed

- Authorization retries will be denied if stored payment status is "undefined", to ensure that users who close the Affirm modal are not redirected to the "order placed" page

## [1.3.1] - 2021-12-09

### Fixed

- Change order of postCallBackResponse
- Update logs for err and info

## [1.3.0] - 2021-10-21

### Added

- Save createPaymentResponse
- Add createSavePaymentResponse Repository

## [1.2.2] - 2021-10-04

### Fixed

- Always get authorization from storage.

## [1.2.1] - 2021-02-04

### Added

- Logging

## [1.2.0] - 2021-01-29

### Added

- Added idempotency key

## [1.1.22] - 2020-11-11

### Changed

- Default amount field for Katapult orders

## [1.1.21] - 2020-11-06

### Changed

- Changed Katapult production url from api.katapult.com to katapult.com

## [1.1.20] - 2020-08-28

### Added

- For Katapult refunds, look up the Discount amount

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
