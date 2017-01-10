Member variables in network classes can have a prefix indicating where they can be
used.
- s: Server only
- c: Client only
- l: Used independently on each local copy
- e: Shared between client and server (either through syncvar or manually)
- p: Valid only on the client with local authority
  - May also appear in classes associated with a local authority object