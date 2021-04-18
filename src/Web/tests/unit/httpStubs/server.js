import {rest} from 'msw';
import {setupServer} from 'msw/node'

export const server = setupServer(rest.get('/api/meetings/:id', (req, res, ctx) => {
    const {id} = req.params;
    console.log(`przechwycilem ${id}`);
    const data = {
      id: id,
      title: 'jakistam'
    };
    return res(ctx.status(200), ctx.json(data));
  }));
