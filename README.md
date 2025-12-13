# Card Wars Battle Engine Design Philosphy

## Overall action flow
**Inputs**
- `Input` -> Queue `Resolvers`

**Resolvers**
- `Resolvers` -> Commit Blocks (`BlockBatch`)
- `Resolvers` -> Queue more `Resolvers`
- `Resolvers` -> Raise `Events`

**Events**
- `Events` -> Queue `Resolvers` via the individual handlers
- `Events` -> Queue `Resolver` via the event resolver at the very end
- `Events` -> Modify `Event Object` which the event resolver uses to resolve it

**Block & Block Batches**
- `Blocks` -> Modify `State`

## Modding support and easy mod content addition
Each part of the engine is split into various `Services` and `Dispatchers`. To easily modify the engine, we can inject the `Dispatchers` to register any custom handler types.

The engine can receive any type of `Inputs` and produce any types of `Blocks`.
- The type of `Inputs` it can process is based on what is registered in the handler.
- The type of `Blocks` it can produce is absed on what us registered in the handler.

### Plans
- To use reflection to register the dispatchers?
- We haven't created our "content" registry portion. It should work via injecting the content definition into the engine as well.
-  The planned Id system for the content definition would be via `namespace:name/of/item_we_want_to_add`
- Plan to also turn the dispatcher to also follow the id system? But depends if the performance-wise
