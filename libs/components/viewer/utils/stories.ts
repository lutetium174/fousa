const storyModules = import.meta.glob("/src/**/*.stories.{ts,tsx}");

export interface StoryFile {
  path: string;
  name: string;
}

export interface StoryInfo {
  name: string;
  isFunction: boolean;
}

export const getStoryFiles = async (): Promise<StoryFile[]> => {
  const stories: StoryFile[] = [];

  for (const path in storyModules) {
    if (path.includes("stories.")) {
      const fileName = path
        .split("/")
        .pop()!
        .replace(".stories.tsx", "")
        .replace(".stories.ts", "");

      stories.push({
        path: path,
        name: fileName,
      });
    }
  }

  return stories;
};

export const getStoryNames = async (path: string): Promise<StoryInfo[]> => {
  const module = await import(/* @vite-ignore */ path);
  const storyNames: StoryInfo[] = [];

  for (const key in module) {
    if (key !== 'default' && module[key as keyof typeof module]) {
      const story = module[key as keyof typeof module];
      storyNames.push({
        name: key,
        isFunction: typeof story === 'function',
      });
    }
  }

  return storyNames;
};
