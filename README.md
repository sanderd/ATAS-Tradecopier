
# Documentation

Welcome to the ATAS Trade Copy System documentation.

## Disclaimer
No warranty is included, the software is provided AS-IS. 
The author will not be held responsible for any issues, nor for the potential financial (or other) losses that are the result of that.
That being said, the author is using this software daily for personal needs.

## Quick Start

- [Installation Guide](docs/installation.md) - Get up and running quickly
- [Configuration Guide](docs/configuration.md) - Set up your trading environment
- [Quick screen recording demonstrating ATAS Strategy setup](https://youtu.be/_MHOYJV1rhU)

## System Overview

The ATAS Trade Copy System consists of two main components:

1. **ATAS Strategy (BroadcastOrderEvents)** - Captures trade events from ATAS platform and transmits them to **Order Event Hub**
2. **Order Event Hub** - Web application that manages trade routing and execution

## Features

- Real-time trade copying from ATAS to multiple brokers
- Web-based management interface
- Position sizing and risk management
- Multiple copy strategies
- Real-time monitoring and notifications
- ProjectX integration

## Getting Started

1. Follow the [Installation Guide](docs/installation.md) to set up the system
2. Configure your trading setup using the [Configuration Guide](docs/configuration.md)
3. Start trading and monitor through the web interface

## Usage notes
1. Currently tested vendors: **Topstep** and **Lucid Trading**. All currently listed vendors on the ProjectX website were added.
2. The copying happens in order-mode, and therefor, discrepancies can occur between source and destination. Currently, **no** monitoring of these discrepancies is supported.

## Wishlist / missing features
1. Implement OCO (One-Cancels-Other) orders with ProjectX-native OCO orders. Current edge-cases are possible where eg. TP of ATAS was hit, SL order got removed. If that TP wasn't hit on ProjectX yet, the SL still gets removed.
2. Re-sync functionality that recovers the current position & orders when sync was lost.
3. Monitor discrepancies between source and destination.

## Development

This documentation covers user installation and configuration. For development documentation, see the source code and inline comments.
