import { Component, computed, effect, resource, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly newTitle = signal('');

  protected readonly todos = resource({
    loader: async ({ abortSignal }) => (await import('./todo.api')).fetchTodos(abortSignal)
  });

  protected readonly items = computed(() => this.todos.value() ?? []);
  protected readonly pendingCount = computed(() => this.items().filter(x => !x.isDone).length);

  constructor() {
    effect(() => {
      const err = this.todos.error();
      if (err) console.error(err);
    });
  }

  protected async add() {
    const title = this.newTitle().trim();
    if (!title) return;

    const api = await import('./todo.api');
    await api.addTodo(title);
    this.newTitle.set('');
    this.todos.reload();
  }

  protected async toggle(id: number, isDone: boolean) {
    const api = await import('./todo.api');
    await api.setTodoDone(id, isDone);
    this.todos.reload();
  }

  protected async rename(id: number, title: string) {
    const api = await import('./todo.api');
    await api.renameTodo(id, title);
    this.todos.reload();
  }

  protected async remove(id: number) {
    const api = await import('./todo.api');
    await api.deleteTodo(id);
    this.todos.reload();
  }
}
