import { Input } from "components";
import { createSignal, type JSX } from "solid-js";
import type { TextInputProps } from "components";

export default {
  title: "Components/Input",
  component: Input,
  argTypes: {
    size: {
      control: "select",
      options: ["sm", "md", "lg"],
    },
    disabled: {
      control: "boolean",
    },
    invalid: {
      control: "boolean",
    },
    filled: {
      control: "boolean",
    },
  },
};

type Story = TextInputProps;

export const Basic: Story = {
  placeholder: "Enter text...",
  label: "Basic Input",
};

export const WithLabel: Story = {
  label: "Username",
  placeholder: "Enter your username",
};

export const WithHelperText: Story = {
  label: "Email",
  placeholder: "Enter your email",
  helperText: "We'll never share your email",
};

export const WithError: Story = {
  label: "Password",
  placeholder: "Enter password",
  errorText: "Password is required",
  invalid: true,
};

export const Disabled: Story = {
  label: "Disabled Input",
  placeholder: "Cannot edit",
  disabled: true,
};

export const Filled: Story = {
  label: "Filled Input",
  placeholder: "Filled style",
  filled: true,
};

export const Small: Story = {
  label: "Small Input",
  placeholder: "Small size",
  size: "sm",
};

export const Large: Story = {
  label: "Large Input",
  placeholder: "Large size",
  size: "lg",
};

// --- Interactive Stories: Retrieving and Using Input Text ---

/**
 * Controlled Input - demonstrates retrieving text value with onChange
 * The input value is stored in state and updated on every change
 */
export const ControlledInput = () => {
  const [value, setValue] = createSignal("");
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label="Controlled Input"
        placeholder="Type something..."
        value={value()}
        onChange={(e) => setValue(e.target.value)}
      />
      <div style={{ 
        padding: "12px", 
        background: "var(--bg-subtle)", 
        "border-radius": "4px",
        "font-family": "inherit"
      }}>
        <p style={{ margin: 0, color: "var(--text-muted)" }}>
          Current value: <strong style={{ color: "var(--text-color)" }}>{value() || "(empty)"}</strong>
        </p>
      </div>
    </div>
  );
};

/**
 * Real-time Character Counter - demonstrates using input value for live feedback
 */
export const CharacterCounter = () => {
  const [text, setText] = createSignal("");
  const [maxLength, setMaxLength] = createSignal(50);
  
  const characterCount = () => text().length;
  const remainingChars = () => maxLength() - characterCount();
  const isOverLimit = () => characterCount() > maxLength();
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label={`Character Counter (${characterCount()}/${maxLength()})`}
        placeholder="Type to see character count..."
        value={text()}
        onChange={(e) => setText(e.target.value)}
        errorText={isOverLimit() ? `Over limit by ${characterCount() - maxLength()} characters` : ""}
        invalid={isOverLimit()}
      />
      <div style={{ 
        display: "flex", 
        justifyContent: "space-between",
        padding: "8px 12px", 
        background: "var(--bg-subtle)", 
        "border-radius": "4px"
      }}>
        <span style={{ color: "var(--text-muted)" }}>Characters: {characterCount()}</span>
        <span style={{ 
          color: isOverLimit() ? "var(--red-500)" : "var(--text-muted)" 
        }}>
          Remaining: {remainingChars()}
        </span>
      </div>
    </div>
  );
};

/**
 * Form Submission - demonstrates retrieving input value on form submit
 */
export const FormSubmission = () => {
  const [name, setName] = createSignal("");
  const [email, setEmail] = createSignal("");
  const [submittedData, setSubmittedData] = createSignal<{name: string; email: string} | null>(null);
  
  const handleSubmit = (e: Event) => {
    e.preventDefault();
    setSubmittedData({ name: name(), email: email() });
  };
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <form onSubmit={handleSubmit} style={{ display: "flex", "flex-direction": "column", gap: "16px" }}>
        <Input
          label="Name"
          placeholder="Enter your name"
          value={name()}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <Input
          label="Email"
          placeholder="Enter your email"
          type="email"
          value={email()}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <button 
          type="submit" 
          style={{ 
            padding: "12px 24px", 
            background: "var(--primary)", 
            color: "var(--primary-contrast)",
            border: "none", 
            "border-radius": "4px",
            cursor: "pointer",
            "font-family": "inherit",
            "font-size": "inherit"
          }}
        >
          Submit
        </button>
      </form>
      
      {submittedData() && (
        <div style={{ 
          padding: "16px", 
          background: "var(--primary-50)", 
          "border-radius": "4px",
          border: "1px solid var(--primary-200)"
        }}>
          <h4 style={{ margin: "0 0 8px 0", color: "var(--primary)" }}>Submitted Data:</h4>
          <p style={{ margin: "4px 0" }}><strong>Name:</strong> {submittedData()!.name}</p>
          <p style={{ margin: "4px 0" }}><strong>Email:</strong> {submittedData()!.email}</p>
        </div>
      )}
    </div>
  );
};

/**
 * Two-way Binding - demonstrates using onInput for immediate value updates
 */
export const TwoWayBinding = () => {
  const [searchQuery, setSearchQuery] = createSignal("");
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label="Search"
        placeholder="Search for something..."
        value={searchQuery()}
        onInput={(e) => setSearchQuery(e.target.value)}
        iconLeft={<span>🔍</span>}
      />
      
      {searchQuery() ? (
        <div style={{ 
          padding: "12px", 
          background: "var(--bg-subtle)", 
          "border-radius": "4px"
        }}>
          <p style={{ margin: 0, color: "var(--text-muted)" }}>
            Search results for: <strong style={{ color: "var(--primary)" }}>{searchQuery()}</strong>
          </p>
        </div>
      ) : (
        <p style={{ color: "var(--text-muted)", "font-size": "0.875rem" }}>
          Start typing to see search results...
        </p>
      )}
    </div>
  );
};

/**
 * Multiple Inputs with Dynamic Validation - demonstrates using values for validation
 */
export const DynamicValidation = () => {
  const [password, setPassword] = createSignal("");
  const [confirmPassword, setConfirmPassword] = createSignal("");
  
  const passwordsMatch = () => password() === confirmPassword() && password().length > 0;
  const showFeedback = () => confirmPassword().length > 0;
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label="Password"
        type="password"
        placeholder="Enter password"
        value={password()}
        onChange={(e) => setPassword(e.target.value)}
      />
      <Input
        label="Confirm Password"
        type="password"
        placeholder="Confirm password"
        value={confirmPassword()}
        onChange={(e) => setConfirmPassword(e.target.value)}
        errorText={showFeedback() && !passwordsMatch() ? "Passwords do not match" : ""}
        invalid={showFeedback() && !passwordsMatch()}
        helperText={showFeedback() && passwordsMatch() ? "Passwords match!" : ""}
      />
      
      {passwordsMatch() && password().length > 0 && (
        <p style={{ 
          margin: 0, 
          color: "var(--green-600)", 
          "font-size": "0.875rem",
          "text-align": "center"
        }}>
          ✅ Passwords match!
        </p>
      )}
    </div>
  );
};

/**
 * Conditional Logic Based on Input - demonstrates using input value for conditional rendering
 */
export const ConditionalRendering = () => {
  const [age, setAge] = createSignal("");
  const ageNumber = () => parseInt(age()) || 0;
  
  const isChild = () => ageNumber() < 13;
  const isTeen = () => ageNumber() >= 13 && ageNumber() < 20;
  const isAdult = () => ageNumber() >= 20 && ageNumber() < 65;
  const isSenior = () => ageNumber() >= 65;
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label="Age"
        type="number"
        placeholder="Enter your age"
        value={age()}
        onChange={(e) => setAge(e.target.value)}
        min="0"
        max="120"
      />
      
      {ageNumber() > 0 ? (
        <div style={{ 
          padding: "12px", 
          background: "var(--bg-subtle)", 
          "border-radius": "4px"
        }}>
          {isChild() && (
            <p style={{ margin: 0, color: "var(--orange-500)" }}>
              <strong>Child:</strong> You're eligible for junior programs!
            </p>
          )}
          {isTeen() && (
            <p style={{ margin: 0, color: "var(--blue-500)" }}>
              <strong>Teen:</strong> You can apply for student discounts!
            </p>
          )}
          {isAdult() && (
            <p style={{ margin: 0, color: "var(--green-500)" }}>
              <strong>Adult:</strong> Full access granted!
            </p>
          )}
          {isSenior() && (
            <p style={{ margin: 0, color: "var(--purple-500)" }}>
              <strong>Senior:</strong> Special senior benefits available!
            </p>
          )}
        </div>
      ) : (
        <p style={{ color: "var(--text-muted)", "font-size": "0.875rem" }}>
          Enter your age to see available benefits
        </p>
      )}
    </div>
  );
};

/**
 * Form with Reset - demonstrates retrieving and resetting input values
 */
export const FormWithReset = () => {
  const [formData, setFormData] = createSignal({
    username: "",
    bio: ""
  });
  
  /*const handleInputChange = (field: keyof typeof formData(), value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
  };
  
  const handleReset = () => {
    setFormData({ username: "", bio: "" });
  };*/
  
  const hasContent = () => formData().username.length > 0 || formData().bio.length > 0;
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <Input
        label="Username"
        placeholder="Enter username"
        value={formData().username}
        //onChange={(e) => handleInputChange("username", e.target.value)}
      />
      <Input
        label="Bio"
        placeholder="Tell us about yourself"
        value={formData().bio}
        //onChange={(e) => handleInputChange("bio", e.target.value)}
      />
      
      <div style={{ display: "flex", gap: "8px" }}>
        <button 
          type="button" 
          //onClick={handleReset}
          disabled={!hasContent()}
          style={{ 
            padding: "8px 16px", 
            background: !hasContent() ? "var(--bg-emphasis)" : "var(--surface-300)",
            color: !hasContent() ? "var(--text-muted)" : "var(--text-color)",
            border: "none", 
            "border-radius": "4px",
            cursor: !hasContent() ? "not-allowed" : "pointer",
            "font-family": "inherit"
          }}
        >
          Reset
        </button>
        <button 
          type="button" 
          style={{ 
            padding: "8px 16px", 
            background: "var(--primary)", 
            color: "var(--primary-contrast)",
            border: "none", 
            "border-radius": "4px",
            cursor: "pointer",
            "font-family": "inherit"
          }}
        >
          Save
        </button>
      </div>
      
      {hasContent() && (
        <div style={{ 
          padding: "12px", 
          background: "var(--bg-subtle)", 
          "border-radius": "4px",
          "font-size": "0.875rem"
        }}>
          <p style={{ margin: 0, color: "var(--text-muted)" }}>
            Preview: <strong style={{ color: "var(--text-color)" }}>{formData().username}</strong>
            {formData().bio && <>, {formData().bio}</>}
          </p>
        </div>
      )}
    </div>
  );
};

/**
 * Search with Clear Button - demonstrates using input value with dynamic clear functionality
 */
export const SearchWithClear = () => {
  const [searchTerm, setSearchTerm] = createSignal("");
  
  const handleClear = () => {
    setSearchTerm("");
  };
  
  return (
    <div style={{ display: "flex", "flex-direction": "column", gap: "16px", "max-width": "400px" }}>
      <div style={{ display: "flex", gap: "8px", "align-items": "center" }}>
        <Input
          label="Search"
          placeholder="Search..."
          value={searchTerm()}
          onChange={(e) => setSearchTerm(e.target.value)}
          iconLeft={<span>🔍</span>}
          style={{ flex: 1 }}
        />
        {searchTerm().length > 0 && (
          <button
            onClick={handleClear}
            style={{ 
              padding: "8px 12px", 
              background: "transparent",
              border: "1px solid var(--border)",
              "border-radius": "4px",
              cursor: "pointer",
              "font-family": "inherit",
              "margin-top": "28px"  /* Align with input */
            }}
          >
            Clear
          </button>
        )}
      </div>
      
      <div style={{ 
        padding: "12px", 
        background: "var(--bg-subtle)", 
        "border-radius": "4px"
      }}>
        {searchTerm().length > 0 ? (
          <p style={{ margin: 0, color: "var(--text-color)" }}>
            Showing results for: <strong>{searchTerm()}</strong>
          </p>
        ) : (
          <p style={{ margin: 0, color: "var(--text-muted)" }}>
            No search term entered
          </p>
        )}
      </div>
    </div>
  );
};
