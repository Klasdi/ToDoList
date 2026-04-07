export type Todo = {
  id: number;
  title: string;
  isDone: boolean;
  createdAt: string;
  updatedAt?: string | null;
};

const endpoint = 'http://localhost:5038/graphql';

type GraphQLError = { message: string };
type GraphQLResponse<T> = { data?: T; errors?: GraphQLError[] };

async function gql<T>(query: string, variables?: Record<string, unknown>, signal?: AbortSignal): Promise<T> {
  const res = await fetch(endpoint, {
    method: 'POST',
    headers: { 'content-type': 'application/json' },
    body: JSON.stringify({ query, variables }),
    signal
  });

  const json = (await res.json()) as GraphQLResponse<T>;

  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  if (json.errors?.length) throw new Error(json.errors.map(e => e.message).join('\n'));
  if (!json.data) throw new Error('Empty GraphQL data');

  return json.data;
}

export async function fetchTodos(signal?: AbortSignal): Promise<Todo[]> {
  const query = `
    query Todos {
      todos {
        id
        title
        isDone
        createdAt
        updatedAt
      }
    }
  `;
  const data = await gql<{ todos: Todo[] }>(query, undefined, signal);
  return data.todos;
}

export async function addTodo(title: string): Promise<Todo> {
  const mutation = `
    mutation AddTodo($title: String!) {
      addTodo(title: $title) {
        id
        title
        isDone
        createdAt
        updatedAt
      }
    }
  `;
  const data = await gql<{ addTodo: Todo }>(mutation, { title });
  return data.addTodo;
}

export async function setTodoDone(id: number, isDone: boolean): Promise<Todo> {
  const mutation = `
    mutation SetTodoDone($id: Int!, $isDone: Boolean!) {
      setTodoDone(id: $id, isDone: $isDone) {
        id
        title
        isDone
        createdAt
        updatedAt
      }
    }
  `;
  const data = await gql<{ setTodoDone: Todo }>(mutation, { id, isDone });
  return data.setTodoDone;
}

export async function renameTodo(id: number, title: string): Promise<Todo> {
  const mutation = `
    mutation RenameTodo($id: Int!, $title: String!) {
      renameTodo(id: $id, title: $title) {
        id
        title
        isDone
        createdAt
        updatedAt
      }
    }
  `;
  const data = await gql<{ renameTodo: Todo }>(mutation, { id, title });
  return data.renameTodo;
}

export async function deleteTodo(id: number): Promise<boolean> {
  const mutation = `
    mutation DeleteTodo($id: Int!) {
      deleteTodo(id: $id)
    }
  `;
  const data = await gql<{ deleteTodo: boolean }>(mutation, { id });
  return data.deleteTodo;
}
