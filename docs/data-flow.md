# Data Flow

## Core Principle

```
ReservationSystem manages the lifecycle of asset bookings:
  Browse -> Reserve -> Approve -> Pickup (feedback) -> Return (feedback) -> Complete
```

## Reservation Lifecycle

```
                    User creates reservation
                              |
                       [Pending]
                        /      \
              Manager approves  Manager rejects
                      |              |
              [Accepted]         [Rejected]
             /          \             |
    Pickup feedback   Cancel         [Done]
      (2-day window)    |
            |        [Canceled]
     [Return feedback]
       (2-day window)
            |
         [Done]
```

### Creating a Reservation (Employee)

1. User browses available assets (soft-deleted excluded)
2. Selects asset, dates, destination, accepts terms
3. System checks for date conflicts using Serializable isolation level
4. Reservation saved with Pending status
5. Email sent to user (confirmation)
6. Email sent to group managers and admins for approval

### Approval / Rejection (Manager)

1. Manager views pending reservations (scoped to their user groups)
2. Approves or rejects the reservation
3. Email sent to user with status update

### Pickup and Return Feedback

1. Hangfire hourly job creates FeedbackLog entries at pickup/return time
2. User provides feedback within the configured window (default: 2 days)
3. If provided -> status becomes Provided
4. If expired -> status becomes Expired, alert sent to managers

## Feedback Mechanism

```
                    Pickup date                  Return date
                         |                            |
                    +----+----+                 +-----+-----+
                    |         |                 |           |
              [FeedbackLog]   |           [FeedbackLog]     |
              Kind = Pickup   |           Kind = Return     |
              Status: Pending |           Status: Pending   |
                    |         |                 |           |
           +--------+--+      |        +--------+--+        |
           |           |      |        |           |        |
       Provided    Expired    |    Provided    Expired      |
           |           |      |        |           |        |
      (2-day     (alert to    |   (2-day     (alert to      |
       window)    managers)   |    window)    managers)     |
                              |                            |
                         Reservation completed             
```

## Group Access Scoping

```
                   Admin (full access)
                         |
              +----------+----------+
              |                     |
         Manager A             Manager B
         group: [1, 2]         group: [3]
              |                     |
    Can see reservations      Can see reservations
    for users in groups       for users in groups
    1 and 2 only              3 only
```

- Managers can only see reservations/faults from users in their assigned groups
- Admins bypass all scoping

## Soft Delete

- Assets and ApplicationUsers use soft-delete (IsDeleted flag)
- Global query filters exclude deleted entities from normal queries