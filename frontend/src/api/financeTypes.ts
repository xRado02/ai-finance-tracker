export type TransactionType = 'Income' | 'Expense';

export type CategoryResponse = {
  id: string;
  name: string;
  appliesTo: TransactionType | null;
};

export type ProfileSettingsResponse = {
  displayName: string;
  initialBalance: number;
};

export type CreateTransactionRequest = {
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
};

export type TransactionResponse = {
  id: string;
  amount: number;
  type: TransactionType;
  transactionDate: string;
  description: string | null;
  categoryId: string;
  categoryName: string;
};

export type CreateRecurringTransactionRequest = {
  amount: number;
  type: TransactionType;
  categoryId: string;
  description: string | null;
  isActive: boolean;
};

export type UpdateRecurringTransactionStatusRequest = {
  isActive: boolean;
};

export type RecurringTransactionResponse = {
  id: string;
  amount: number;
  type: TransactionType;
  categoryId: string;
  categoryName: string;
  description: string | null;
  isActive: boolean;
};

export type GenerateRecurringTransactionsResponse = {
  month: string;
  generatedCount: number;
  skippedCount: number;
  transactions: TransactionResponse[];
};

export type CreateGoalRequest = {
  name: string;
  targetAmount: number;
};

export type GoalResponse = {
  id: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  progressPercentage: number;
};

export type GoalForecastStatus = 'Forecastable' | 'Achieved' | 'NoData' | 'NoPositiveSurplus';

export type GoalForecastResponse = {
  goalId: string;
  name: string;
  targetAmount: number;
  currentAmount: number;
  remainingAmount: number;
  averageMonthlySurplus: number | null;
  estimatedMonths: number | null;
  estimatedDate: string | null;
  status: GoalForecastStatus;
};

export type ExpenseCategorySummary = {
  categoryName: string;
  amount: number;
};

export type IncomeCategorySummary = ExpenseCategorySummary;

export type DashboardGoalSummary = GoalResponse;

export type DashboardSummaryResponse = {
  initialBalance: number;
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  expenseCategories: ExpenseCategorySummary[];
  goals: DashboardGoalSummary[];
};

export type MonthlySummaryResponse = {
  year: number;
  month: number;
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  expenseCategories: ExpenseCategorySummary[];
  incomeCategories: IncomeCategorySummary[];
};

export type ApiError = {
  message: string;
  status?: number;
};
