import { createSignal, type JSX, createEffect } from "solid-js";
import "./App.css";
import { Navbar as ComponentsNavbar, type NavbarItem } from "components";
import { getStoryFiles, getStoryNames } from "../utils/stories";

const loadStory = async (path: string, storyName?: string): Promise<JSX.Element> => {
  const module = await import(/* @vite-ignore */ path);
  
  if (module.default && typeof module.default === 'object' && 'component' in module.default) {
    const StoryComponent = module.default.component as any;
    const storyKeys = Object.keys(module).filter(key => key !== 'default' && module[key as keyof typeof module]);
    
    // Use the specified story name or fall back to the first one
    const targetStoryName = storyName || storyKeys[0];
    const story = module[targetStoryName as keyof typeof module];
    
    // Handle both function stories (() => JSX) and object stories ({ props })
    if (typeof story === 'function') {
      return story();
    } else if (typeof story === 'object' && story !== null) {
      // Extract children and other props to handle Solid.js children prop correctly
      const { children, ...props } = story as any;
      if (children !== undefined) {
        return <StoryComponent {...props}>{children}</StoryComponent>;
      }
      return <StoryComponent {...props} />;
    }
  }
  
  return module.default;
};

function App() {
  const [storyFiles, setStoryFiles] = createSignal<{ path: string; name: string }[]>([]);
  const [selectedFile, setSelectedFile] = createSignal<string | null>(null);
  const [selectedStoryName, setSelectedStoryName] = createSignal<string | null>(null);
  const [storyComponent, setStoryComponent] = createSignal<JSX.Element>(<div>Select a story</div>);
  const [navbarItems, setNavbarItems] = createSignal<NavbarItem[]>([]);

  // Load story files and their story names on mount
  createEffect(() => {
    getStoryFiles().then(async files => {
      setStoryFiles(files);
      
      // Load all story names for all files
      const itemsWithSubItems = await Promise.all(
        files.map(async file => {
          const stories = await getStoryNames(file.path);
          const storyItems: NavbarItem[] = stories.map(story => ({
            key: `${file.path}:${story.name}`,
            label: story.name,
          }));
          return {
            key: file.path,
            label: file.name,
            subItems: storyItems,
          };
        })
      );
      
      setNavbarItems(itemsWithSubItems);
    });
  });

  const handleNavbarSelect = async (key: string) => {
    // Check if key is a file path or a story key
    if (key.includes(":")) {
      const [filePath, storyName] = key.split(":");
      setSelectedFile(filePath);
      setSelectedStoryName(storyName);
      const story = await loadStory(filePath, storyName);
      setStoryComponent(story);
    } else {
      // It's a file path - load first story
      setSelectedFile(key);
      setSelectedStoryName(null);
      const story = await loadStory(key);
      setStoryComponent(story);
    }
  };

  const buildSelectedKey = () => {
    const file = selectedFile();
    const story = selectedStoryName();
    if (file && story) {
      return `${file}:${story}`;
    }
    return selectedFile() || undefined;
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <div style={{ width: "250px", borderRight: "1px solid #ccc" }}>
        <ComponentsNavbar
          items={navbarItems()}
          onSelect={handleNavbarSelect}
          selectedKey={buildSelectedKey()}
        />
      </div>
      <div style={{ flex: 1, padding: "16px" }}>
        {storyComponent()}
      </div>
    </div>
  );
}

export default App;
